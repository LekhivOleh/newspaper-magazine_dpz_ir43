using CURSOVA.Application.Abstractions.Sharing;
using CURSOVA.Infrastructure.Sharing.Networks;

namespace CURSOVA.Infrastructure.Sharing;

// Adapters: each one translates our ShareContent into the call its network client expects.

public class TelegramShareAdapter(TelegramShareApi api) : IShareTarget
{
    public string Key => "telegram";
    public string DisplayName => "Telegram";

    public string BuildShareUrl(ShareContent content) => api.ComposeShareUri(content.Url, content.Title).AbsoluteUri;
}

public class TwitterShareAdapter(TwitterIntentClient client) : IShareTarget
{
    public string Key => "x";
    public string DisplayName => "X";

    public string BuildShareUrl(ShareContent content) =>
        client.BuildTweetIntent(new TweetDraft(content.Title, content.Url, content.Hashtags.ToList()));
}

public class FacebookShareAdapter : IShareTarget
{
    public string Key => "facebook";
    public string DisplayName => "Facebook";

    public string BuildShareUrl(ShareContent content) => FacebookSharer.SharerFor(new Uri(content.Url));
}

public class LinkedInShareAdapter : IShareTarget
{
    public string Key => "linkedin";
    public string DisplayName => "LinkedIn";

    public string BuildShareUrl(ShareContent content) => new LinkedInShareBuilder().ForPage(content.Url).Build();
}

public class WhatsAppShareAdapter(WhatsAppLinkFactory factory) : IShareTarget
{
    public string Key => "whatsapp";
    public string DisplayName => "WhatsApp";

    public string BuildShareUrl(ShareContent content) => factory.Create($"{content.Title}\n{content.Url}");
}

public class EmailShareAdapter(MailtoComposer composer) : IShareTarget
{
    public string Key => "email";
    public string DisplayName => "Email";

    public string BuildShareUrl(ShareContent content) =>
        composer.Compose(new MailDraft(content.Title, $"{content.Summary}\n\nЧитати на TechPulse: {content.Url}".TrimStart()));
}
