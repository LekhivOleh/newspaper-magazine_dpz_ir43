using CURSOVA.Domain.Subscriptions;

namespace CURSOVA.Application.Abstractions.Persistence;

public record SubscriberRow(Subscription Subscription, string Email, string DisplayName);

public interface ISubscriptionRepository : IRepository<Subscription>
{
    Task<Subscription?> GetByUserAsync(int userId, CancellationToken ct = default);
    Task<Subscription?> GetWithPaymentsAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<SubscriberRow>> ListWithUsersAsync(CancellationToken ct = default);
    Task<int> TotalRevenueAsync(CancellationToken ct = default);
}
