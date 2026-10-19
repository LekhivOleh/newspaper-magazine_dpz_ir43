using CURSOVA.Domain.Common;

namespace CURSOVA.Domain.Notifications;

public enum NotificationKind
{
    NewIssue,
    ExclusiveContent,
    PartnerPromo
}

public abstract class Notification : Entity
{
    public const int TitleMaxLength = 200;
    public const int MessageMaxLength = 600;

    public int UserId { get; private set; }
    public string Title { get; private set; } = null!;
    public string Message { get; private set; } = null!;
    public string Link { get; private set; } = null!;
    public string? ImageUrl { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ReadAt { get; private set; }

    public abstract NotificationKind Kind { get; }
    public bool IsRead => ReadAt is not null;

    protected Notification() { }

    protected Notification(int userId, string title, string message, string link, string? imageUrl, DateTime createdAt)
    {
        if (userId <= 0)
            throw new DomainException("Сповіщення має мати отримувача.");
        if (!link.StartsWith('/') || link.StartsWith("//") || link.StartsWith("/\\"))
            throw new DomainException("Посилання сповіщення має вести на сторінку журналу.");

        UserId = userId;
        Title = Guard.NotEmpty(title, "Заголовок сповіщення", TitleMaxLength);
        Message = Guard.NotEmpty(message, "Текст сповіщення", MessageMaxLength);
        Link = link;
        ImageUrl = Guard.Optional(imageUrl, "Зображення", 500);
        CreatedAt = createdAt;
    }

    public void MarkRead(DateTime now) => ReadAt ??= now;
}

public class NewIssueNotification : Notification
{
    public int IssueId { get; private set; }

    public override NotificationKind Kind => NotificationKind.NewIssue;

    private NewIssueNotification() { }

    public NewIssueNotification(int userId, int issueId, string title, string message, string link, string? imageUrl, DateTime createdAt)
        : base(userId, title, message, link, imageUrl, createdAt) => IssueId = issueId;
}

public class ExclusiveContentNotification : Notification
{
    public int ArticleId { get; private set; }

    public override NotificationKind Kind => NotificationKind.ExclusiveContent;

    private ExclusiveContentNotification() { }

    public ExclusiveContentNotification(int userId, int articleId, string title, string message, string link, string? imageUrl, DateTime createdAt)
        : base(userId, title, message, link, imageUrl, createdAt) => ArticleId = articleId;
}

public class PartnerPromoNotification : Notification
{
    public int AdId { get; private set; }
    public int? PartnerId { get; private set; }

    public override NotificationKind Kind => NotificationKind.PartnerPromo;

    private PartnerPromoNotification() { }

    public PartnerPromoNotification(int userId, int adId, int? partnerId, string title, string message, string link, string? imageUrl, DateTime createdAt)
        : base(userId, title, message, link, imageUrl, createdAt)
    {
        AdId = adId;
        PartnerId = partnerId;
    }
}
