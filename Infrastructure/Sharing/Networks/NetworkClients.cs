using System.Text.Encodings.Web;

namespace CURSOVA.Infrastructure.Sharing.Networks;

// Adaptees: each social network exposes its own, mutually incompatible way of building a share link.
// They are modelled like third-party SDKs — we treat their interfaces as fixed and adapt to them.

public sealed class TelegramShareApi
{
    public Uri ComposeShareUri(string link, string message) =>
        new($"https://t.me/share/url?url={Encode(link)}&text={Encode(message)}");

    private static string Encode(string value) => UrlEncoder.Default.Encode(value);
}

public sealed record TweetDraft(string Status, string Link, IReadOnlyCollection<string> Tags);

public sealed class TwitterIntentClient
{
    public string BuildTweetIntent(TweetDraft draft)
    {
        var url = $"https://twitter.com/intent/tweet?text={UrlEncoder.Default.Encode(draft.Status)}&url={UrlEncoder.Default.Encode(draft.Link)}";
        return draft.Tags.Count == 0 ? url : url + "&hashtags=" + UrlEncoder.Default.Encode(string.Join(",", draft.Tags));
    }
}

/// <summary>Facebook only accepts the page address — text comes from the page's own meta tags.</summary>
public static class FacebookSharer
{
    public static string SharerFor(Uri page) =>
        $"https://www.facebook.com/sharer/sharer.php?u={UrlEncoder.Default.Encode(page.AbsoluteUri)}";
}

public sealed class LinkedInShareBuilder
{
    private string? _page;

    public LinkedInShareBuilder ForPage(string url)
    {
        _page = url;
        return this;
    }

    public string Build() =>
        $"https://www.linkedin.com/sharing/share-offsite/?url={UrlEncoder.Default.Encode(_page ?? throw new InvalidOperationException("Page is required."))}";
}

/// <summary>WhatsApp takes a single pre-composed message; the link must be part of the text.</summary>
public sealed class WhatsAppLinkFactory
{
    public string Create(string fullMessage) => $"https://wa.me/?text={Uri.EscapeDataString(fullMessage)}";
}

public sealed record MailDraft(string Subject, string Body);

public sealed class MailtoComposer
{
    public string Compose(MailDraft draft) =>
        $"mailto:?subject={Uri.EscapeDataString(draft.Subject)}&body={Uri.EscapeDataString(draft.Body)}";
}
