using CURSOVA.Application.Abstractions.Identity;
using CURSOVA.Application.Abstractions.Persistence;
using CURSOVA.Application.Common;
using CURSOVA.Domain.Common;
using CURSOVA.Domain.Subscriptions;
using CURSOVA.Domain.Subscriptions.States;

namespace CURSOVA.Application.Subscriptions;

public record PlanDto(string Key, string Title, int PriceUah, int Days);

public record PaymentDto(PaymentKind Kind, string PlanKey, int AmountUah, DateTime PaidAt);

public record SubscriptionDto(
    SubscriptionStatus Status,
    string StatusTitle,
    bool HasPremium,
    PlanDto? Plan,
    DateTime? StartedAt,
    DateTime? CurrentPeriodEnd,
    IReadOnlyCollection<SubscriptionAction> AllowedActions,
    IReadOnlyList<PaymentDto> Payments);

public record SubscriberDto(int SubscriptionId, int UserId, string Email, string DisplayName, SubscriptionDto Subscription, int TotalPaidUah);

public record SubscriptionStatsDto(int PremiumReaders, int AutoRenewing, int TotalRevenueUah);

public record SubscribeRequest(string Plan);

public interface ISubscriptionService
{
    IReadOnlyList<PlanDto> Plans { get; }
    Task<SubscriptionDto> GetMineAsync(CancellationToken ct = default);
    Task<bool> HasPremiumAccessAsync(CancellationToken ct = default);
    Task<SubscriptionDto> SubscribeAsync(string planKey, CancellationToken ct = default);
    Task<SubscriptionDto> CancelAsync(CancellationToken ct = default);
    Task<SubscriptionDto> ResumeAsync(CancellationToken ct = default);

    Task<IReadOnlyList<SubscriberDto>> ListSubscribersAsync(CancellationToken ct = default);
    Task<SubscriptionStatsDto> GetStatsAsync(CancellationToken ct = default);
    Task GiftAsync(int userId, int days, CancellationToken ct = default);
    Task EndPeriodNowAsync(int subscriptionId, CancellationToken ct = default);
}

public class SubscriptionService(IUnitOfWork uow, ICurrentUser currentUser, TimeProvider clock) : ISubscriptionService
{
    private bool? _premiumCache;

    private DateTime Now => clock.GetUtcNow().UtcDateTime;

    public IReadOnlyList<PlanDto> Plans => SubscriptionPlan.All.Select(ToDto).ToList();

    public async Task<SubscriptionDto> GetMineAsync(CancellationToken ct = default) =>
        ToDto(await LoadMineAsync(createIfMissing: false, ct) ?? new Subscription(RequireUser()));

    /// <summary>Cheap to call repeatedly within one request — the answer is cached.</summary>
    public async Task<bool> HasPremiumAccessAsync(CancellationToken ct = default)
    {
        if (_premiumCache is { } cached)
            return cached;
        if (currentUser.Id is null)
            return (_premiumCache = false).Value;

        var subscription = await LoadMineAsync(createIfMissing: false, ct);
        return (_premiumCache = subscription?.HasPremiumAccess(Now) ?? false).Value;
    }

    public Task<SubscriptionDto> SubscribeAsync(string planKey, CancellationToken ct = default)
    {
        var plan = SubscriptionPlan.Find(planKey) ?? throw new DomainException($"Невідомий тариф «{planKey}».");
        return ChangeMineAsync(s => s.Subscribe(plan, Now), ct);
    }

    public Task<SubscriptionDto> CancelAsync(CancellationToken ct = default) => ChangeMineAsync(s => s.Cancel(Now), ct);

    public Task<SubscriptionDto> ResumeAsync(CancellationToken ct = default) => ChangeMineAsync(s => s.Resume(Now), ct);

    public async Task<IReadOnlyList<SubscriberDto>> ListSubscribersAsync(CancellationToken ct = default)
    {
        var rows = await uow.Subscriptions.ListWithUsersAsync(ct);
        if (rows.Aggregate(false, (changed, r) => r.Subscription.Refresh(Now) | changed))
            await uow.SaveChangesAsync(ct);

        return rows.Select(r => new SubscriberDto(
            r.Subscription.Id, r.Subscription.UserId, r.Email, r.DisplayName, ToDto(r.Subscription),
            r.Subscription.Payments.Sum(p => p.AmountUah))).ToList();
    }

    public async Task<SubscriptionStatsDto> GetStatsAsync(CancellationToken ct = default)
    {
        var subscribers = await ListSubscribersAsync(ct);
        return new SubscriptionStatsDto(
            subscribers.Count(s => s.Subscription.HasPremium),
            subscribers.Count(s => s.Subscription.Status == SubscriptionStatus.Active),
            await uow.Subscriptions.TotalRevenueAsync(ct));
    }

    public async Task GiftAsync(int userId, int days, CancellationToken ct = default)
    {
        var subscription = await uow.Subscriptions.GetByUserAsync(userId, ct);
        if (subscription is null)
        {
            subscription = new Subscription(userId);
            uow.Subscriptions.Add(subscription);
        }
        else
        {
            subscription.Refresh(Now);
        }

        subscription.Gift(days, Now);
        await uow.SaveChangesAsync(ct);
    }

    public async Task EndPeriodNowAsync(int subscriptionId, CancellationToken ct = default)
    {
        var subscription = await uow.Subscriptions.GetWithPaymentsAsync(subscriptionId, ct)
                           ?? throw NotFoundException.For<Subscription>(subscriptionId);
        subscription.EndPeriodNow(Now);
        subscription.Refresh(Now);
        await uow.SaveChangesAsync(ct);
    }

    private async Task<SubscriptionDto> ChangeMineAsync(Action<Subscription> change, CancellationToken ct)
    {
        var subscription = (await LoadMineAsync(createIfMissing: true, ct))!;
        change(subscription);
        await uow.SaveChangesAsync(ct);
        _premiumCache = null;
        return ToDto(subscription);
    }

    private async Task<Subscription?> LoadMineAsync(bool createIfMissing, CancellationToken ct)
    {
        var userId = RequireUser();
        var subscription = await uow.Subscriptions.GetByUserAsync(userId, ct);

        if (subscription is null && createIfMissing)
        {
            subscription = new Subscription(userId);
            uow.Subscriptions.Add(subscription);
        }
        else if (subscription?.Refresh(Now) == true)
        {
            await uow.SaveChangesAsync(ct);
        }

        return subscription;
    }

    private SubscriptionDto ToDto(Subscription s) => new(
        s.Status, s.State.Title, s.HasPremiumAccess(Now), s.Plan is { } plan ? ToDto(plan) : null,
        s.StartedAt, s.CurrentPeriodEnd, s.State.AllowedActions,
        s.Payments.Select(p => new PaymentDto(p.Kind, p.PlanKey, p.AmountUah, p.PaidAt)).ToList());

    private static PlanDto ToDto(SubscriptionPlan p) => new(p.Key, p.Title, p.PriceUah, p.Days);

    private int RequireUser() => currentUser.Id ?? throw new DomainException("Увійдіть, щоб керувати підпискою.");
}
