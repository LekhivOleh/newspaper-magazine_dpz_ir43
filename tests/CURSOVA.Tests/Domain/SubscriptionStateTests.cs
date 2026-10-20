using CURSOVA.Domain.Common;
using CURSOVA.Domain.Subscriptions;
using CURSOVA.Domain.Subscriptions.States;
using static CURSOVA.Tests.TestSupport;

namespace CURSOVA.Tests.Domain;

public class SubscriptionStateTests
{
    private static Subscription Active()
    {
        var subscription = new Subscription(userId: 1);
        subscription.Subscribe(SubscriptionPlan.Monthly, Now);
        return subscription;
    }

    [Fact]
    public void New_subscription_is_free_without_premium()
    {
        var subscription = new Subscription(1);

        Assert.IsType<FreeState>(subscription.State);
        Assert.False(subscription.HasPremiumAccess(Now));
        Assert.Equal([SubscriptionAction.Subscribe], subscription.State.AllowedActions);
    }

    [Fact]
    public void Subscribing_activates_premium_for_the_plan_period()
    {
        var subscription = Active();

        Assert.IsType<ActiveState>(subscription.State);
        Assert.True(subscription.HasPremiumAccess(Now));
        Assert.Equal(Now.AddDays(30), subscription.CurrentPeriodEnd);
    }

    [Fact]
    public void Cancelled_subscription_keeps_access_until_the_period_ends_then_expires()
    {
        var subscription = Active();
        subscription.Cancel(Now.AddDays(5));

        Assert.IsType<CancelledState>(subscription.State);
        Assert.True(subscription.HasPremiumAccess(Now.AddDays(29)));

        subscription.Refresh(Now.AddDays(31));

        Assert.IsType<ExpiredState>(subscription.State);
        Assert.False(subscription.HasPremiumAccess(Now.AddDays(31)));
    }

    [Fact]
    public void Active_subscription_renews_itself_for_every_elapsed_period()
    {
        var subscription = Active();

        var changed = subscription.Refresh(Now.AddDays(65));

        Assert.True(changed);
        Assert.Equal(SubscriptionStatus.Active, subscription.Status);
        Assert.Equal(Now.AddDays(90), subscription.CurrentPeriodEnd);
        Assert.Equal(2, subscription.Payments.Count(p => p.Kind == PaymentKind.Renewal));
    }

    [Fact]
    public void Resume_is_allowed_only_while_the_paid_period_lasts()
    {
        var resumed = Active();
        resumed.Cancel(Now);
        resumed.Resume(Now.AddDays(10));
        Assert.Equal(SubscriptionStatus.Active, resumed.Status);

        var late = Active();
        late.Cancel(Now);
        Assert.Throws<DomainException>(() => late.Resume(Now.AddDays(40)));
    }

    [Fact]
    public void States_reject_actions_that_make_no_sense_for_them()
    {
        Assert.Throws<DomainException>(() => new Subscription(1).Cancel(Now));
        Assert.Throws<DomainException>(() => new Subscription(1).Resume(Now));
        Assert.Throws<DomainException>(() => Active().Resume(Now));
    }

    [Fact]
    public void Expired_subscriber_can_start_again()
    {
        var subscription = Active();
        subscription.Cancel(Now);
        subscription.Refresh(Now.AddDays(31));

        subscription.Subscribe(SubscriptionPlan.Yearly, Now.AddDays(40));

        Assert.Equal(SubscriptionStatus.Active, subscription.Status);
        Assert.Equal(Now.AddDays(40 + 365), subscription.CurrentPeriodEnd);
    }
}
