using CURSOVA.Application.Abstractions.Messaging;
using CURSOVA.Application.Abstractions.Persistence;
using CURSOVA.Application.Abstractions.Sharing;

namespace CURSOVA.Application.Notifications;

public interface IAudienceResolver
{
    Task<IReadOnlyList<NotificationRecipient>> ResolveAsync(NotificationAudience audience, CancellationToken ct = default);
}

public class AudienceResolver(IUnitOfWork uow, TimeProvider clock) : IAudienceResolver
{
    private IReadOnlyList<NotificationRecipient>? _everyone;

    public async Task<IReadOnlyList<NotificationRecipient>> ResolveAsync(NotificationAudience audience, CancellationToken ct = default)
    {
        _everyone ??= await uow.Notifications.ListRecipientsAsync(ct);
        var now = clock.GetUtcNow().UtcDateTime;

        bool HasPremium(NotificationRecipient r) => r.Subscription?.HasPremiumAccess(now) == true;

        return audience switch
        {
            NotificationAudience.PremiumSubscribers => _everyone.Where(HasPremium).ToList(),
            NotificationAudience.FreeReaders => _everyone.Where(r => !HasPremium(r)).ToList(),
            _ => _everyone
        };
    }
}

/// <summary>Observer: stores a notification for every reader in the event's audience (the bell in the header).</summary>
public class InAppNotificationObserver(
    INotificationFactories factories,
    IAudienceResolver audience,
    IUnitOfWork uow,
    TimeProvider clock) : IMagazineEventObserver
{
    public async Task OnEventAsync(MagazineEvent e, CancellationToken ct = default)
    {
        if (factories.For(e) is not { } factory)
            return;

        var recipients = await audience.ResolveAsync(factory.Audience, ct);
        if (recipients.Count == 0)
            return;

        uow.Notifications.AddRange(factory.CreateFor(e, recipients.Select(r => r.UserId), clock.GetUtcNow().UtcDateTime));
        await uow.SaveChangesAsync(ct);
    }
}

/// <summary>Observer: turns the same notifications into emails, for the event types worth an email.</summary>
public class EmailNotificationObserver(
    INotificationFactories factories,
    IAudienceResolver audience,
    IEmailSender email,
    IAppUrlProvider urls,
    TimeProvider clock) : IMagazineEventObserver
{
    public async Task OnEventAsync(MagazineEvent e, CancellationToken ct = default)
    {
        if (factories.For(e) is not { SendsEmail: true } factory)
            return;

        var recipients = (await audience.ResolveAsync(factory.Audience, ct))
            .Where(r => !string.IsNullOrWhiteSpace(r.Email))
            .ToList();
        var notifications = factory.CreateFor(e, recipients.Select(r => r.UserId), clock.GetUtcNow().UtcDateTime);

        foreach (var (recipient, notification) in recipients.Zip(notifications))
        {
            var body = $"Вітаємо, {recipient.DisplayName}!\n\n{notification.Message}\n\nЧитати: {urls.Absolute(notification.Link)}\n\n— Редакція TechPulse";
            await email.SendAsync(new EmailMessage(recipient.Email, notification.Title, body), ct);
        }
    }
}
