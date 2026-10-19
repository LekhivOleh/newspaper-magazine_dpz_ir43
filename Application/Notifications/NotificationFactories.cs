using CURSOVA.Domain.Notifications;

namespace CURSOVA.Application.Notifications;

public enum NotificationAudience
{
    Everyone,
    PremiumSubscribers,
    FreeReaders
}

/// <summary>
/// Factory Method: the creator. It knows who should receive an event and how to fan it out,
/// while each subclass decides which notification class to instantiate and what it says.
/// </summary>
public abstract class NotificationFactory
{
    public abstract NotificationAudience Audience { get; }
    public virtual bool SendsEmail => true;

    public abstract bool CanCreate(MagazineEvent e);

    public IReadOnlyList<Notification> CreateFor(MagazineEvent e, IEnumerable<int> userIds, DateTime now) =>
        userIds.Select(userId => CreateNotification(e, userId, now)).ToList();

    protected abstract Notification CreateNotification(MagazineEvent e, int userId, DateTime now);

    protected static string Shorten(string text, int maxLength = 280) =>
        text.Length <= maxLength ? text : text[..(maxLength - 1)].TrimEnd() + "…";
}

public abstract class NotificationFactory<TEvent> : NotificationFactory where TEvent : MagazineEvent
{
    public override bool CanCreate(MagazineEvent e) => e is TEvent;

    protected override Notification CreateNotification(MagazineEvent e, int userId, DateTime now) =>
        Create((TEvent)e, userId, now);

    protected abstract Notification Create(TEvent e, int userId, DateTime now);
}

public class NewIssueNotificationFactory : NotificationFactory<IssuePublished>
{
    public override NotificationAudience Audience => NotificationAudience.Everyone;

    protected override Notification Create(IssuePublished e, int userId, DateTime now) =>
        new NewIssueNotification(userId, e.IssueId,
            $"Новий випуск: {e.Title}",
            Shorten(e.Description),
            $"/issues/{Uri.EscapeDataString(e.Slug)}",
            e.CoverImageUrl, now);
}

public class ExclusiveContentNotificationFactory : NotificationFactory<ExclusiveContentPublished>
{
    public override NotificationAudience Audience => NotificationAudience.PremiumSubscribers;

    protected override Notification Create(ExclusiveContentPublished e, int userId, DateTime now) =>
        new ExclusiveContentNotification(userId, e.ArticleId,
            e.IsPremiumArticle ? $"Лише для Premium: {e.Title}" : $"Ексклюзивні матеріали: {e.Title}",
            Shorten(e.Lead),
            $"/article/{Uri.EscapeDataString(e.Slug)}",
            e.CoverImageUrl, now);
}

/// <summary>Premium means no ads, so promotions only go to free readers and only inside the site.</summary>
public class PartnerPromoNotificationFactory : NotificationFactory<PartnerPromotionLaunched>
{
    public override NotificationAudience Audience => NotificationAudience.FreeReaders;
    public override bool SendsEmail => false;

    protected override Notification Create(PartnerPromotionLaunched e, int userId, DateTime now) =>
        new PartnerPromoNotification(userId, e.AdId, e.PartnerId,
            e.PartnerName is null ? e.Headline : $"{e.PartnerName}: {e.Headline}",
            Shorten(string.IsNullOrWhiteSpace(e.Body) ? "Спеціальна пропозиція для читачів TechPulse." : e.Body),
            $"/ads/{e.AdId}/click",
            e.ImageUrl, now);
}

public interface INotificationFactories
{
    NotificationFactory? For(MagazineEvent e);
}

public class NotificationFactories(IEnumerable<NotificationFactory> factories) : INotificationFactories
{
    public NotificationFactory? For(MagazineEvent e) => factories.FirstOrDefault(f => f.CanCreate(e));
}
