using CURSOVA.Application.Abstractions.Persistence;
using CURSOVA.Application.Common;
using CURSOVA.Domain.Notifications;
using Microsoft.EntityFrameworkCore;

namespace CURSOVA.Infrastructure.Persistence.Repositories;

public class NotificationRepository(MagazineDbContext db) : INotificationRepository
{
    public async Task<PagedResult<Notification>> ListForUserAsync(int userId, int page, int pageSize, CancellationToken ct = default)
    {
        var query = db.Notifications.AsNoTracking().Where(n => n.UserId == userId);
        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(n => n.CreatedAt)
            .ThenByDescending(n => n.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<Notification>(items, page, pageSize, total);
    }

    public Task<int> CountUnreadAsync(int userId, CancellationToken ct = default) =>
        db.Notifications.CountAsync(n => n.UserId == userId && n.ReadAt == null, ct);

    public Task<Notification?> GetForUserAsync(int id, int userId, CancellationToken ct = default) =>
        db.Notifications.FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId, ct);

    public async Task<IReadOnlyList<Notification>> ListUnreadAsync(int userId, CancellationToken ct = default) =>
        await db.Notifications.Where(n => n.UserId == userId && n.ReadAt == null).ToListAsync(ct);

    public async Task<IReadOnlyList<NotificationRecipient>> ListRecipientsAsync(CancellationToken ct = default) =>
        (await db.Users.AsNoTracking()
            .GroupJoin(db.Subscriptions.AsNoTracking(), u => u.Id, s => s.UserId, (u, s) => new { u, s })
            .SelectMany(x => x.s.DefaultIfEmpty(), (x, s) => new { x.u.Id, x.u.Email, x.u.DisplayName, Subscription = s })
            .ToListAsync(ct))
        .Select(x => new NotificationRecipient(x.Id, x.Email ?? "", x.DisplayName, x.Subscription))
        .ToList();

    public void AddRange(IEnumerable<Notification> notifications) => db.Notifications.AddRange(notifications);
}
