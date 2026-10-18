using CURSOVA.Application.Abstractions.Identity;
using CURSOVA.Application.Abstractions.Persistence;
using CURSOVA.Application.Articles;
using CURSOVA.Application.Common;
using CURSOVA.Domain.Articles;
using CURSOVA.Domain.Common;
using CURSOVA.Domain.Profiles;

namespace CURSOVA.Application.Profiles;

public record SavedArticleDto(ArticleSummaryDto Article, DateTime SavedAt);

public interface ISavedArticleService
{
    Task<bool> IsSavedAsync(int articleId, CancellationToken ct = default);
    Task SaveAsync(int articleId, CancellationToken ct = default);
    Task RemoveAsync(int articleId, CancellationToken ct = default);
    Task<bool> ToggleAsync(int articleId, CancellationToken ct = default);
    Task<PagedResult<SavedArticleDto>> ListMineAsync(int page, CancellationToken ct = default);
}

public class SavedArticleService(IUnitOfWork uow, ICurrentUser currentUser) : ISavedArticleService
{
    private const int PageSize = 12;

    public async Task<bool> IsSavedAsync(int articleId, CancellationToken ct = default) =>
        currentUser.Id is { } userId && await uow.SavedArticles.FindAsync(userId, articleId, ct) is not null;

    public async Task SaveAsync(int articleId, CancellationToken ct = default)
    {
        var userId = RequireUser();
        if (await uow.SavedArticles.FindAsync(userId, articleId, ct) is not null)
            return;

        var article = await uow.Articles.GetByIdAsync(articleId, ct) ?? throw NotFoundException.For<Article>(articleId);
        if (!article.IsPublished)
            throw new DomainException("Зберегти можна лише опубліковану статтю.");

        uow.SavedArticles.Add(new SavedArticle(userId, articleId));
        await uow.SaveChangesAsync(ct);
    }

    public async Task RemoveAsync(int articleId, CancellationToken ct = default)
    {
        if (await uow.SavedArticles.FindAsync(RequireUser(), articleId, ct) is not { } saved)
            return;

        uow.SavedArticles.Remove(saved);
        await uow.SaveChangesAsync(ct);
    }

    public async Task<bool> ToggleAsync(int articleId, CancellationToken ct = default)
    {
        if (await IsSavedAsync(articleId, ct))
        {
            await RemoveAsync(articleId, ct);
            return false;
        }

        await SaveAsync(articleId, ct);
        return true;
    }

    public async Task<PagedResult<SavedArticleDto>> ListMineAsync(int page, CancellationToken ct = default)
    {
        var saved = await uow.SavedArticles.ListAsync(RequireUser(), Math.Max(1, page), PageSize, ct);
        var articles = (await uow.Articles.GetByIdsAsync(saved.Items.Select(s => s.ArticleId), ct))
            .ToDictionary(a => a.Id, a => a.ToSummary());

        return new PagedResult<SavedArticleDto>(
            saved.Items.Where(s => articles.ContainsKey(s.ArticleId))
                .Select(s => new SavedArticleDto(articles[s.ArticleId], s.SavedAt))
                .ToList(),
            saved.Page, saved.PageSize, saved.TotalCount);
    }

    private int RequireUser() => currentUser.Id ?? throw new DomainException("Увійдіть, щоб зберігати статті.");
}
