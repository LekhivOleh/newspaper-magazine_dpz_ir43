using CURSOVA.Application.Abstractions.Identity;
using CURSOVA.Application.Abstractions.Persistence;
using CURSOVA.Application.Common;
using CURSOVA.Domain.Common;
using CURSOVA.Domain.Notifications;

namespace CURSOVA.Application.Notifications;

public record NotificationDto(
    int Id,
    NotificationKind Kind,
    string Title,
    string Message,
    string Link,
    string? ImageUrl,
    DateTime CreatedAt,
    bool IsRead);

public interface INotificationService
{
    Task<PagedResult<NotificationDto>> ListMineAsync(int page, CancellationToken ct = default);
    Task<int> CountUnreadAsync(CancellationToken ct = default);
    Task<NotificationDto> MarkReadAsync(int id, CancellationToken ct = default);
    Task<int> MarkAllReadAsync(CancellationToken ct = default);
}

public class NotificationService(IUnitOfWork uow, ICurrentUser currentUser, TimeProvider clock) : INotificationService
{
    private const int PageSize = 20;

    public async Task<PagedResult<NotificationDto>> ListMineAsync(int page, CancellationToken ct = default) =>
        (await uow.Notifications.ListForUserAsync(RequireUser(), Math.Max(1, page), PageSize, ct)).Map(ToDto);

    public async Task<int> CountUnreadAsync(CancellationToken ct = default) =>
        currentUser.Id is { } userId ? await uow.Notifications.CountUnreadAsync(userId, ct) : 0;

    public async Task<NotificationDto> MarkReadAsync(int id, CancellationToken ct = default)
    {
        var notification = await uow.Notifications.GetForUserAsync(id, RequireUser(), ct)
                           ?? throw NotFoundException.For<Notification>(id);
        notification.MarkRead(Now);
        await uow.SaveChangesAsync(ct);
        return ToDto(notification);
    }

    public async Task<int> MarkAllReadAsync(CancellationToken ct = default)
    {
        var unread = await uow.Notifications.ListUnreadAsync(RequireUser(), ct);
        foreach (var notification in unread)
            notification.MarkRead(Now);

        await uow.SaveChangesAsync(ct);
        return unread.Count;
    }

    private DateTime Now => clock.GetUtcNow().UtcDateTime;

    private int RequireUser() => currentUser.Id ?? throw new DomainException("Увійдіть, щоб переглядати сповіщення.");

    private static NotificationDto ToDto(Notification n) =>
        new(n.Id, n.Kind, n.Title, n.Message, n.Link, n.ImageUrl, n.CreatedAt, n.IsRead);
}
