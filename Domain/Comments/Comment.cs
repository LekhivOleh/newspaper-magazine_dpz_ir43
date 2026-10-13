using CURSOVA.Domain.Common;

namespace CURSOVA.Domain.Comments;

public enum CommentStatus
{
    Pending,
    Published,
    Rejected
}

public class Comment : Entity
{
    public const int BodyMaxLength = 2000;

    public int ArticleId { get; private set; }
    public int UserId { get; private set; }
    public string AuthorName { get; private set; } = null!;
    public string Body { get; private set; } = null!;
    public CommentStatus Status { get; private set; }
    public string? ModerationNote { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Comment() { }

    public Comment(int articleId, int userId, string authorName, string body, CommentStatus status, string? moderationNote)
    {
        ArticleId = articleId;
        UserId = userId;
        AuthorName = Guard.NotEmpty(authorName, "Автор", 100);
        Body = Guard.NotEmpty(body, "Текст коментаря", BodyMaxLength);
        Status = status;
        ModerationNote = moderationNote;
        CreatedAt = DateTime.UtcNow;
    }

    public void Approve()
    {
        Status = CommentStatus.Published;
        ModerationNote = null;
    }

    public void Reject(string? reason)
    {
        Status = CommentStatus.Rejected;
        ModerationNote = reason;
    }
}
