using System.ComponentModel.DataAnnotations;
using CURSOVA.Application.Abstractions.Identity;
using CURSOVA.Application.Abstractions.Persistence;
using CURSOVA.Application.Comments.Moderation;
using CURSOVA.Application.Common;
using CURSOVA.Domain.Articles;
using CURSOVA.Domain.Comments;
using CURSOVA.Domain.Common;

namespace CURSOVA.Application.Comments;

public record CommentDto(int Id, int ArticleId, string AuthorName, string Body, CommentStatus Status, string? ModerationNote,
    DateTime CreatedAt, bool IsMine);

public record PostCommentRequest([Required, MaxLength(Comment.BodyMaxLength)] string Body);

public record PostCommentResult(CommentDto? Comment, ModerationDecision Decision, string? Message);

public interface ICommentService
{
    Task<IReadOnlyList<CommentDto>> ListForArticleAsync(int articleId, CancellationToken ct = default);
    Task<PostCommentResult> PostAsync(int articleId, string body, CancellationToken ct = default);
    Task DeleteAsync(int commentId, CancellationToken ct = default);

    Task<PagedResult<CommentDto>> ListForModerationAsync(CommentStatus? status, int page, CancellationToken ct = default);
    Task<int> CountPendingAsync(CancellationToken ct = default);
    Task ApproveAsync(int commentId, CancellationToken ct = default);
    Task RejectAsync(int commentId, string? reason, CancellationToken ct = default);
}

public class CommentService(IUnitOfWork uow, ICurrentUser currentUser, ICommentModerator moderator) : ICommentService
{
    public async Task<IReadOnlyList<CommentDto>> ListForArticleAsync(int articleId, CancellationToken ct = default) =>
        (await uow.Comments.ListForArticleAsync(articleId, currentUser.Id, ct)).Select(ToDto).ToList();

    public async Task<PostCommentResult> PostAsync(int articleId, string body, CancellationToken ct = default)
    {
        var userId = currentUser.Id ?? throw new DomainException("Увійдіть, щоб коментувати.");
        var article = await uow.Articles.GetByIdAsync(articleId, ct) ?? throw NotFoundException.For<Article>(articleId);
        if (!article.IsPublished)
            throw new DomainException("Коментувати можна лише опубліковані статті.");

        var recent = await uow.Comments.CountByUserSinceAsync(userId, DateTime.UtcNow - RateLimitHandler.Window, ct);
        var previous = await uow.Comments.GetRecentBodiesByUserAsync(userId, take: 5, ct);

        var result = moderator.Moderate(new CommentDraft(body ?? "", recent, previous));
        if (result.Decision == ModerationDecision.Reject)
            return new PostCommentResult(null, result.Decision, result.Reason);

        var status = result.Decision == ModerationDecision.Approve ? CommentStatus.Published : CommentStatus.Pending;
        var comment = new Comment(articleId, userId, currentUser.DisplayName ?? "Читач", result.Text, status, result.Reason);
        uow.Comments.Add(comment);
        await uow.SaveChangesAsync(ct);

        return new PostCommentResult(ToDto(comment), result.Decision, result.Reason);
    }

    public async Task DeleteAsync(int commentId, CancellationToken ct = default)
    {
        var comment = await LoadAsync(commentId, ct);
        if (comment.UserId != currentUser.Id && !currentUser.IsAdmin)
            throw new DomainException("Можна видаляти лише власні коментарі.");

        uow.Comments.Remove(comment);
        await uow.SaveChangesAsync(ct);
    }

    public async Task<PagedResult<CommentDto>> ListForModerationAsync(CommentStatus? status, int page, CancellationToken ct = default) =>
        (await uow.Comments.ListForModerationAsync(status, Math.Max(1, page), 20, ct)).Map(ToDto);

    public Task<int> CountPendingAsync(CancellationToken ct = default) => uow.Comments.CountPendingAsync(ct);

    public async Task ApproveAsync(int commentId, CancellationToken ct = default)
    {
        (await LoadAsync(commentId, ct)).Approve();
        await uow.SaveChangesAsync(ct);
    }

    public async Task RejectAsync(int commentId, string? reason, CancellationToken ct = default)
    {
        (await LoadAsync(commentId, ct)).Reject(string.IsNullOrWhiteSpace(reason) ? "Відхилено модератором." : reason.Trim());
        await uow.SaveChangesAsync(ct);
    }

    private async Task<Comment> LoadAsync(int id, CancellationToken ct) =>
        await uow.Comments.GetByIdAsync(id, ct) ?? throw NotFoundException.For<Comment>(id);

    private CommentDto ToDto(Comment c) =>
        new(c.Id, c.ArticleId, c.AuthorName, c.Body, c.Status, c.ModerationNote, c.CreatedAt, c.UserId == currentUser.Id);
}
