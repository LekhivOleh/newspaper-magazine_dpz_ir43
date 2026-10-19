using CURSOVA.Domain.Common;

namespace CURSOVA.Domain.Advertising;

public class Ad : Entity
{
    public int? PartnerId { get; private set; }
    public string Headline { get; private set; } = null!;
    public string? Body { get; private set; }
    public string? ImageUrl { get; private set; }
    public string TargetUrl { get; private set; } = null!;
    public string CallToAction { get; private set; } = null!;

    /// <summary>Show only next to articles of this category; null means everywhere.</summary>
    public int? CategoryId { get; private set; }

    public bool IsActive { get; private set; } = true;
    public int Impressions { get; private set; }
    public int Clicks { get; private set; }

    private Ad() { }

    public Ad(string headline, string targetUrl, string callToAction, string? body = null, string? imageUrl = null,
        int? partnerId = null, int? categoryId = null)
    {
        Update(headline, targetUrl, callToAction, body, imageUrl, partnerId, categoryId, isActive: true);
    }

    public void Update(string headline, string targetUrl, string callToAction, string? body, string? imageUrl,
        int? partnerId, int? categoryId, bool isActive)
    {
        Headline = Guard.NotEmpty(headline, "Заголовок оголошення", 120);
        TargetUrl = AdUrl.Require(targetUrl, "Посилання оголошення");
        CallToAction = Guard.NotEmpty(callToAction, "Текст кнопки", 40);
        Body = Guard.Optional(body, "Текст оголошення", 300);
        ImageUrl = Guard.Optional(imageUrl, "Зображення", 500);
        PartnerId = partnerId is > 0 ? partnerId : null;
        CategoryId = categoryId is > 0 ? categoryId : null;
        IsActive = isActive;
    }

    public bool TargetsCategory(int categoryId) => CategoryId is null || CategoryId == categoryId;

    public void RegisterImpression() => Impressions++;

    public void RegisterClick() => Clicks++;

    public double ClickThroughRate => Impressions == 0 ? 0 : (double)Clicks / Impressions;
}
