using CURSOVA.Domain.Common;

namespace CURSOVA.Domain.Subscriptions.States;

public enum SubscriptionAction
{
    Subscribe,
    Cancel,
    Resume
}

/// <summary>
/// Base state: by default nothing is allowed and there is no Premium access.
/// Concrete states override only what makes sense for them.
/// States hold no data, so one shared instance per status is enough.
/// </summary>
public abstract class SubscriptionState
{
    private static readonly Dictionary<SubscriptionStatus, SubscriptionState> States =
        new SubscriptionState[] { new FreeState(), new ActiveState(), new CancelledState(), new ExpiredState() }
            .ToDictionary(s => s.Status);

    public static SubscriptionState For(SubscriptionStatus status) => States[status];

    public abstract SubscriptionStatus Status { get; }
    public abstract string Title { get; }

    public abstract IReadOnlyCollection<SubscriptionAction> AllowedActions { get; }

    public virtual bool HasPremiumAccess(Subscription s, DateTime now) => false;

    public virtual void Subscribe(Subscription s, SubscriptionPlan plan, DateTime now) =>
        throw new DomainException($"Неможливо оформити підписку у стані «{Title}».");

    public virtual void Cancel(Subscription s, DateTime now) =>
        throw new DomainException("Немає активної підписки, яку можна скасувати.");

    public virtual void Resume(Subscription s, DateTime now) =>
        throw new DomainException("Відновити можна лише скасовану підписку, що ще діє.");

    public virtual void Refresh(Subscription s, DateTime now) { }
}

public class FreeState : SubscriptionState
{
    public override SubscriptionStatus Status => SubscriptionStatus.Free;
    public override string Title => "Безкоштовний доступ";
    public override IReadOnlyCollection<SubscriptionAction> AllowedActions => [SubscriptionAction.Subscribe];

    public override void Subscribe(Subscription s, SubscriptionPlan plan, DateTime now)
    {
        s.StartPeriod(plan, now, PaymentKind.Purchase);
        s.TransitionTo(SubscriptionStatus.Active);
    }
}

public class ActiveState : SubscriptionState
{
    public override SubscriptionStatus Status => SubscriptionStatus.Active;
    public override string Title => "Premium активний";
    public override IReadOnlyCollection<SubscriptionAction> AllowedActions => [SubscriptionAction.Subscribe, SubscriptionAction.Cancel];

    public override bool HasPremiumAccess(Subscription s, DateTime now) => true;

    /// <summary>Paying again while active prepays another period on top of the current one.</summary>
    public override void Subscribe(Subscription s, SubscriptionPlan plan, DateTime now) =>
        s.ExtendPeriod(plan, now, PaymentKind.Purchase);

    public override void Cancel(Subscription s, DateTime now) => s.TransitionTo(SubscriptionStatus.Cancelled);

    /// <summary>Auto-renewal: every elapsed period is charged again (demo payment).</summary>
    public override void Refresh(Subscription s, DateTime now)
    {
        var plan = s.Plan ?? SubscriptionPlan.Monthly;
        while (s.CurrentPeriodEnd is { } end && end <= now)
            s.ExtendPeriod(plan, end, PaymentKind.Renewal);
    }
}

public class CancelledState : SubscriptionState
{
    public override SubscriptionStatus Status => SubscriptionStatus.Cancelled;
    public override string Title => "Premium без автопродовження";
    public override IReadOnlyCollection<SubscriptionAction> AllowedActions => [SubscriptionAction.Subscribe, SubscriptionAction.Resume];

    public override bool HasPremiumAccess(Subscription s, DateTime now) => s.CurrentPeriodEnd > now;

    public override void Subscribe(Subscription s, SubscriptionPlan plan, DateTime now)
    {
        s.ExtendPeriod(plan, now, PaymentKind.Purchase);
        s.TransitionTo(SubscriptionStatus.Active);
    }

    public override void Resume(Subscription s, DateTime now)
    {
        if (!HasPremiumAccess(s, now))
            throw new DomainException("Оплачений період уже завершився — оформіть підписку знову.");
        if (s.Plan is null)
            throw new DomainException("Подарунковий доступ не можна продовжувати автоматично — оберіть тариф.");

        s.TransitionTo(SubscriptionStatus.Active);
    }

    public override void Refresh(Subscription s, DateTime now)
    {
        if (s.CurrentPeriodEnd <= now)
            s.TransitionTo(SubscriptionStatus.Expired);
    }
}

public class ExpiredState : SubscriptionState
{
    public override SubscriptionStatus Status => SubscriptionStatus.Expired;
    public override string Title => "Premium завершився";
    public override IReadOnlyCollection<SubscriptionAction> AllowedActions => [SubscriptionAction.Subscribe];

    public override void Subscribe(Subscription s, SubscriptionPlan plan, DateTime now)
    {
        s.StartPeriod(plan, now, PaymentKind.Purchase);
        s.TransitionTo(SubscriptionStatus.Active);
    }
}
