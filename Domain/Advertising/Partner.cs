using CURSOVA.Domain.Common;

namespace CURSOVA.Domain.Advertising;

/// <summary>A brand the magazine works with: sponsored articles and ad campaigns.</summary>
public class Partner : Entity
{
    public string Name { get; private set; } = null!;
    public string? Tagline { get; private set; }
    public string? LogoUrl { get; private set; }
    public string WebsiteUrl { get; private set; } = null!;
    public bool IsActive { get; private set; } = true;

    private Partner() { }

    public Partner(string name, string websiteUrl, string? tagline = null, string? logoUrl = null)
    {
        Update(name, websiteUrl, tagline, logoUrl, isActive: true);
    }

    public void Update(string name, string websiteUrl, string? tagline, string? logoUrl, bool isActive)
    {
        Name = Guard.NotEmpty(name, "Назва партнера", 100);
        WebsiteUrl = AdUrl.Require(websiteUrl, "Сайт партнера");
        Tagline = Guard.Optional(tagline, "Слоган", 200);
        LogoUrl = Guard.Optional(logoUrl, "Логотип", 500);
        IsActive = isActive;
    }
}

internal static class AdUrl
{
    /// <summary>Only http(s) links or paths on our own site — no javascript:, data: or protocol-relative URLs in ads.</summary>
    public static string Require(string? url, string field)
    {
        var value = Guard.NotEmpty(url, field, 500);
        var isLocalPath = value.StartsWith('/') && !value.StartsWith("//") && !value.StartsWith("/\\");
        var isWebUrl = Uri.TryCreate(value, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
        if (!isLocalPath && !isWebUrl)
            throw new DomainException($"{field}: потрібне посилання http(s) або шлях на сайті.");
        return value;
    }
}
