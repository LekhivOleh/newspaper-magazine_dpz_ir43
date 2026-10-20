using CURSOVA.Application.Abstractions.Sharing;
using CURSOVA.Infrastructure.Sharing;
using CURSOVA.Infrastructure.Sharing.Networks;

namespace CURSOVA.Tests.Infrastructure;

public class ShareAdapterTests
{
    private static readonly ShareContent Content = new(
        "https://techpulse.test/article/ohliad-noutbuka",
        "Огляд ноутбука",
        "Коротко про головне",
        ["TechPulse", "Ноутбуки"]);

    public static TheoryData<IShareTarget, string> Targets => new()
    {
        { new TelegramShareAdapter(new TelegramShareApi()), "https://t.me/share/url" },
        { new FacebookShareAdapter(), "https://www.facebook.com/sharer" },
        { new TwitterShareAdapter(new TwitterIntentClient()), "https://twitter.com/intent/tweet" },
        { new LinkedInShareAdapter(), "https://www.linkedin.com/" },
        { new WhatsAppShareAdapter(new WhatsAppLinkFactory()), "https://wa.me/" },
        { new EmailShareAdapter(new MailtoComposer()), "mailto:" }
    };

    [Theory]
    [MemberData(nameof(Targets))]
    public void Every_adapter_turns_the_same_content_into_its_network_link(IShareTarget target, string expectedStart)
    {
        var url = target.BuildShareUrl(Content);

        Assert.StartsWith(expectedStart, url);
        Assert.Contains(Uri.EscapeDataString(Content.Url), url);
    }

    [Fact]
    public void Adapters_have_unique_keys()
    {
        var keys = Targets.Select(row => ((IShareTarget)row[0]).Key).ToList();

        Assert.Equal(keys.Count, keys.Distinct().Count());
    }
}
