using CURSOVA.Application.Abstractions.Messaging;
using CURSOVA.Application.Abstractions.Persistence;
using CURSOVA.Application.Abstractions.Sharing;
using CURSOVA.Application.Notifications;
using CURSOVA.Domain.Notifications;
using CURSOVA.Domain.Subscriptions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using static CURSOVA.Tests.TestSupport;

namespace CURSOVA.Tests.Application;

public class NotificationFactoryTests
{
    private static readonly NotificationFactories Factories = new(
    [
        new NewIssueNotificationFactory(),
        new ExclusiveContentNotificationFactory(),
        new PartnerPromoNotificationFactory()
    ]);

    public static TheoryData<MagazineEvent, Type, NotificationAudience, string> Events => new()
    {
        { new IssuePublished(5, "Жовтень", "zhovten", "Опис", null), typeof(NewIssueNotification), NotificationAudience.Everyone, "/issues/zhovten" },
        { new ExclusiveContentPublished(8, "Огляд", "ohliad", "Лід", null, true), typeof(ExclusiveContentNotification), NotificationAudience.PremiumSubscribers, "/article/ohliad" },
        { new PartnerPromotionLaunched(3, 7, "Гаджетарня", "Знижки", null, null), typeof(PartnerPromoNotification), NotificationAudience.FreeReaders, "/ads/3/click" }
    };

    [Theory]
    [MemberData(nameof(Events))]
    public void Each_event_has_a_factory_that_creates_its_own_notification_class(
        MagazineEvent e, Type expectedType, NotificationAudience audience, string link)
    {
        var factory = Factories.For(e)!;

        var notifications = factory.CreateFor(e, [1, 2], Now);

        Assert.Equal(audience, factory.Audience);
        Assert.Equal([1, 2], notifications.Select(n => n.UserId));
        Assert.All(notifications, n =>
        {
            Assert.IsType(expectedType, n);
            Assert.Equal(link, n.Link);
            Assert.False(n.IsRead);
        });
    }

    [Fact]
    public void Exclusive_title_depends_on_what_is_exclusive()
    {
        var factory = new ExclusiveContentNotificationFactory();

        var premium = factory.CreateFor(new ExclusiveContentPublished(1, "А", "a", "Лід", null, IsPremiumArticle: true), [1], Now)[0];
        var media = factory.CreateFor(new ExclusiveContentPublished(1, "А", "a", "Лід", null, IsPremiumArticle: false), [1], Now)[0];

        Assert.StartsWith("Лише для Premium", premium.Title);
        Assert.StartsWith("Ексклюзивні матеріали", media.Title);
    }

    [Fact]
    public void Long_texts_are_shortened_and_slugs_escaped()
    {
        var notification = new NewIssueNotificationFactory()
            .CreateFor(new IssuePublished(1, "Випуск", "a b", new string('я', 1000), null), [1], Now)[0];

        Assert.True(notification.Message.Length <= 280);
        Assert.EndsWith("…", notification.Message);
        Assert.Equal("/issues/a%20b", notification.Link);
    }

    [Fact]
    public void Partner_promos_are_never_emailed()
    {
        Assert.False(new PartnerPromoNotificationFactory().SendsEmail);
        Assert.True(new NewIssueNotificationFactory().SendsEmail);
    }
}

public class MagazineEventObserverTests
{
    private static readonly IssuePublished Issue = new(5, "Жовтень", "zhovten", "Опис", null);

    private sealed class RecordingObserver : IMagazineEventObserver
    {
        public List<MagazineEvent> Received { get; } = [];

        public Task OnEventAsync(MagazineEvent e, CancellationToken ct = default)
        {
            Received.Add(e);
            return Task.CompletedTask;
        }
    }

    private sealed class FailingObserver : IMagazineEventObserver
    {
        public Task OnEventAsync(MagazineEvent e, CancellationToken ct = default) => throw new InvalidOperationException("SMTP down");
    }

    [Fact]
    public async Task Publisher_notifies_every_attached_observer()
    {
        var first = new RecordingObserver();
        var second = new RecordingObserver();
        var publisher = new MagazineEventPublisher([first], NullLogger<MagazineEventPublisher>.Instance);
        publisher.Attach(second);

        await publisher.PublishAsync(Issue);

        Assert.Equal([Issue], first.Received);
        Assert.Equal([Issue], second.Received);
    }

    [Fact]
    public async Task Detached_observer_is_no_longer_notified()
    {
        var observer = new RecordingObserver();
        var publisher = new MagazineEventPublisher([observer], NullLogger<MagazineEventPublisher>.Instance);
        publisher.Detach(observer);

        await publisher.PublishAsync(Issue);

        Assert.Empty(observer.Received);
    }

    [Fact]
    public async Task One_failing_observer_does_not_stop_the_others()
    {
        var after = new RecordingObserver();
        var publisher = new MagazineEventPublisher([new FailingObserver(), after], NullLogger<MagazineEventPublisher>.Instance);

        await publisher.PublishAsync(Issue);

        Assert.Single(after.Received);
    }

    private static NotificationRecipient Recipient(int id, bool premium)
    {
        var subscription = new Subscription(id);
        if (premium)
            subscription.Subscribe(SubscriptionPlan.Monthly, Now.AddDays(-1));
        return new NotificationRecipient(id, $"user{id}@example.com", $"Читач {id}", subscription);
    }

    private static IUnitOfWork UnitOfWorkWith(params NotificationRecipient[] recipients)
    {
        var uow = Substitute.For<IUnitOfWork>();
        uow.Notifications.ListRecipientsAsync(Arg.Any<CancellationToken>()).Returns(recipients);
        return uow;
    }

    private static readonly NotificationFactories Factories = new(
        [new NewIssueNotificationFactory(), new ExclusiveContentNotificationFactory(), new PartnerPromoNotificationFactory()]);

    [Theory]
    [InlineData(NotificationAudience.Everyone, new[] { 1, 2, 3 })]
    [InlineData(NotificationAudience.PremiumSubscribers, new[] { 2 })]
    [InlineData(NotificationAudience.FreeReaders, new[] { 1, 3 })]
    public async Task Audience_is_resolved_from_subscriptions(NotificationAudience audience, int[] expected)
    {
        var uow = UnitOfWorkWith(Recipient(1, false), Recipient(2, true), Recipient(3, false));

        var recipients = await new AudienceResolver(uow, new FixedClock(Now)).ResolveAsync(audience);

        Assert.Equal(expected, recipients.Select(r => r.UserId));
    }

    [Fact]
    public async Task In_app_observer_stores_one_notification_per_recipient()
    {
        var uow = UnitOfWorkWith(Recipient(1, false), Recipient(2, true));
        var clock = new FixedClock(Now);
        var observer = new InAppNotificationObserver(Factories, new AudienceResolver(uow, clock), uow, clock);

        await observer.OnEventAsync(new ExclusiveContentPublished(8, "Огляд", "ohliad", "Лід", null, true));

        uow.Notifications.Received(1).AddRange(Arg.Is<IEnumerable<Notification>>(n => n.Single().UserId == 2));
        await uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Email_observer_mails_the_audience_with_absolute_links()
    {
        var uow = UnitOfWorkWith(Recipient(1, false), Recipient(2, true));
        var email = Substitute.For<IEmailSender>();
        var urls = Substitute.For<IAppUrlProvider>();
        urls.Absolute(Arg.Any<string>()).Returns(call => "https://techpulse.test" + call.Arg<string>());
        var clock = new FixedClock(Now);
        var observer = new EmailNotificationObserver(Factories, new AudienceResolver(uow, clock), email, urls, clock);

        await observer.OnEventAsync(Issue);
        await observer.OnEventAsync(new PartnerPromotionLaunched(3, 7, "Гаджетарня", "Знижки", null, null));

        await email.Received(2).SendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>());
        await email.Received(1).SendAsync(
            Arg.Is<EmailMessage>(m => m.To == "user2@example.com" && m.Body.Contains("https://techpulse.test/issues/zhovten")),
            Arg.Any<CancellationToken>());
    }
}
