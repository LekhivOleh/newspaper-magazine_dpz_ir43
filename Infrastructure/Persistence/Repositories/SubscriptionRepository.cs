using CURSOVA.Application.Abstractions.Persistence;
using CURSOVA.Domain.Subscriptions;
using Microsoft.EntityFrameworkCore;

namespace CURSOVA.Infrastructure.Persistence.Repositories;

public class SubscriptionRepository(MagazineDbContext db) : Repository<Subscription>(db), ISubscriptionRepository
{
    public Task<Subscription?> GetByUserAsync(int userId, CancellationToken ct = default) =>
        Set.Include(s => s.Payments).FirstOrDefaultAsync(s => s.UserId == userId, ct);

    public Task<Subscription?> GetWithPaymentsAsync(int id, CancellationToken ct = default) =>
        Set.Include(s => s.Payments).FirstOrDefaultAsync(s => s.Id == id, ct);

    public async Task<IReadOnlyList<SubscriberRow>> ListWithUsersAsync(CancellationToken ct = default) =>
        (await Set.Include(s => s.Payments)
            .Join(Db.Users, s => s.UserId, u => u.Id, (s, u) => new { s, u.Email, u.DisplayName })
            .OrderByDescending(x => x.s.CurrentPeriodEnd)
            .ToListAsync(ct))
        .Select(x => new SubscriberRow(x.s, x.Email ?? "", x.DisplayName))
        .ToList();

    public Task<int> TotalRevenueAsync(CancellationToken ct = default) =>
        Db.Set<SubscriptionPayment>().SumAsync(p => p.AmountUah, ct);
}
