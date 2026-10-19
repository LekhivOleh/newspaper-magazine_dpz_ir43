using CURSOVA.Application.Common;
using CURSOVA.Domain.Notifications;
using CURSOVA.Domain.Subscriptions;

namespace CURSOVA.Application.Abstractions.Persistence;

public record NotificationRecipient(int UserId, string Email, string DisplayName, Subscription? Subscription);

public interface INotificationRepository
{
    Task<PagedResult<Notification>> ListForUserAsync(int userId, int page, int pageSize, CancellationToken ct = default);
    Task<int> CountUnreadAsync(int userId, CancellationToken ct = default);
    Task<Notification?> GetForUserAsync(int id, int userId, CancellationToken ct = default);
    Task<IReadOnlyList<Notification>> ListUnreadAsync(int userId, CancellationToken ct = default);
    Task<IReadOnlyList<NotificationRecipient>> ListRecipientsAsync(CancellationToken ct = default);
    void AddRange(IEnumerable<Notification> notifications);
}
