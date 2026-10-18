using CURSOVA.Domain.Common;
using CURSOVA.Domain.Subscriptions.States;

namespace CURSOVA.Domain.Subscriptions;

/// <summary>
/// State pattern context: every operation is delegated to the object representing the current status.
/// The status itself is persisted; the matching state object is looked up from it.
/// </summary>
public class Subscription : Entity
{
    private readonly List<SubscriptionPayment> _payments = [];

    public int UserId { get; private set; }
    public SubscriptionStatus Status { get; private set; } = SubscriptionStatus.Free;
    public string? PlanKey { get; private set; }
    public DateTime? StartedAt { get; private set; }
    public DateTime? CurrentPeriodEnd { get; private set; }
    public DateTime? CancelledAt { get; private set; }

    public IReadOnlyList<SubscriptionPayment> Payments => _payments.OrderByDescending(p => p.PaidAt).ToList();

    public SubscriptionState State => SubscriptionState.For(Status);
    public SubscriptionPlan? Plan => SubscriptionPlan.Find(PlanKey);

    private Subscription() { }

    public Subscription(int userId) => UserId = userId;

    public bool HasPremiumAccess(DateTime now) => State.HasPremiumAccess(this, now);

    public void Subscribe(SubscriptionPlan plan, DateTime now) => State.Subscribe(this, plan, now);

    public void Cancel(DateTime now) => State.Cancel(this, now);

    public void Resume(DateTime now) => State.Resume(this, now);

    /// <summary>Applies what time has done: renewals of active subscriptions, expiry of cancelled ones.</summary>
    public bool Refresh(DateTime now)
    {
        var before = (Status, CurrentPeriodEnd);
        State.Refresh(this, now);
        return before != (Status, CurrentPeriodEnd);
    }

    /// <summary>Complimentary access from the editorial team; never auto-renews.</summary>
    public void Gift(int days, DateTime now)
    {
        if (days is < 1 or > 366)
            throw new DomainException("Подарувати можна від 1 до 366 днів.");

        var from = CurrentPeriodEnd is { } end && end > now && HasPremiumAccess(now) ? end : now;
        CurrentPeriodEnd = from.AddDays(days);
        StartedAt ??= now;
        _payments.Add(new SubscriptionPayment(PaymentKind.Gift, PlanKey ?? "gift", 0, now));
        if (Status != SubscriptionStatus.Active)
            TransitionTo(SubscriptionStatus.Cancelled);
    }

    /// <summary>For demos: makes the current period end right now, so the next refresh moves the state on.</summary>
    public void EndPeriodNow(DateTime now)
    {
        if (CurrentPeriodEnd is null)
            throw new DomainException("У цієї підписки немає оплаченого періоду.");
        CurrentPeriodEnd = now;
    }

    // Operations used by the state objects.

    internal void TransitionTo(SubscriptionStatus status)
    {
        Status = status;
        CancelledAt = status == SubscriptionStatus.Cancelled ? DateTime.UtcNow : null;
    }

    internal void StartPeriod(SubscriptionPlan plan, DateTime now, PaymentKind kind)
    {
        PlanKey = plan.Key;
        StartedAt = now;
        CurrentPeriodEnd = now.AddDays(plan.Days);
        _payments.Add(new SubscriptionPayment(kind, plan.Key, plan.PriceUah, now));
    }

    internal void ExtendPeriod(SubscriptionPlan plan, DateTime now, PaymentKind kind)
    {
        PlanKey = plan.Key;
        var from = CurrentPeriodEnd is { } end && end > now ? end : now;
        CurrentPeriodEnd = from.AddDays(plan.Days);
        _payments.Add(new SubscriptionPayment(kind, plan.Key, plan.PriceUah, now));
    }
}

public class SubscriptionPayment : Entity
{
    public int SubscriptionId { get; private set; }
    public PaymentKind Kind { get; private set; }
    public string PlanKey { get; private set; } = null!;
    public int AmountUah { get; private set; }
    public DateTime PaidAt { get; private set; }

    private SubscriptionPayment() { }

    internal SubscriptionPayment(PaymentKind kind, string planKey, int amountUah, DateTime paidAt)
    {
        Kind = kind;
        PlanKey = planKey;
        AmountUah = amountUah;
        PaidAt = paidAt;
    }
}
