using CURSOVA.Application.Advertising;

namespace CURSOVA.Application.Articles.Presentation;

public abstract record PageSegment;

public record ContentSegment(BlockDto Block) : PageSegment;

public record AdSegment(AdDto Ad) : PageSegment;

public record SponsorDisclosureSegment(PartnerDto Partner) : PageSegment;

public record PartnerPromoSegment(PartnerDto Partner, AdDto? Ad) : PageSegment;

public record ArticlePage(ArticleDetailsDto Article, IReadOnlyList<PageSegment> Segments);

public record ReaderContext(bool HasPremium);

/// <summary>Decorator component: turns an article into the sequence of segments the page renders.</summary>
public interface IArticlePageComposer
{
    Task<ArticlePage> ComposeAsync(ArticleDetailsDto article, ReaderContext reader, CancellationToken ct = default);
}

/// <summary>Concrete component: the article's own blocks, nothing added.</summary>
public class PlainArticleComposer : IArticlePageComposer
{
    public Task<ArticlePage> ComposeAsync(ArticleDetailsDto article, ReaderContext reader, CancellationToken ct = default) =>
        Task.FromResult(new ArticlePage(article, article.Blocks.Select(b => (PageSegment)new ContentSegment(b)).ToList()));
}

/// <summary>Base decorator: wraps another composer and by default just delegates to it.</summary>
public abstract class ArticlePageDecorator(IArticlePageComposer inner) : IArticlePageComposer
{
    public virtual Task<ArticlePage> ComposeAsync(ArticleDetailsDto article, ReaderContext reader, CancellationToken ct = default) =>
        inner.ComposeAsync(article, reader, ct);
}

/// <summary>Inserts ad slots between content blocks. Premium readers get the page untouched.</summary>
public class AdSlotsDecorator(IArticlePageComposer inner, IAdvertisingService ads) : ArticlePageDecorator(inner)
{
    public const int BlocksBetweenAds = 2;
    public const int MaxAds = 2;

    public override async Task<ArticlePage> ComposeAsync(ArticleDetailsDto article, ReaderContext reader, CancellationToken ct = default)
    {
        var page = await base.ComposeAsync(article, reader, ct);
        if (reader.HasPremium)
            return page;

        var picked = await ads.PickAsync(article.Summary.Category.Id, MaxAds, ct: ct);
        if (picked.Count == 0)
            return page;

        var segments = new List<PageSegment>();
        var contentSeen = 0;
        var next = 0;
        foreach (var segment in page.Segments)
        {
            segments.Add(segment);
            if (segment is ContentSegment && ++contentSeen % BlocksBetweenAds == 0 && next < picked.Count)
                segments.Add(new AdSegment(picked[next++]));
        }

        // Short articles still show one ad, at the end.
        if (next == 0)
            segments.Add(new AdSegment(picked[0]));

        return page with { Segments = segments };
    }
}

/// <summary>
/// Brand integration: sponsored articles get a disclosure at the top (always — readers must know)
/// and a partner promo at the bottom (hidden for Premium, like any other ad).
/// </summary>
public class SponsoredContentDecorator(IArticlePageComposer inner, IAdvertisingService ads) : ArticlePageDecorator(inner)
{
    public override async Task<ArticlePage> ComposeAsync(ArticleDetailsDto article, ReaderContext reader, CancellationToken ct = default)
    {
        var page = await base.ComposeAsync(article, reader, ct);
        if (article.Summary.SponsorPartnerId is not { } partnerId || await ads.FindActivePartnerAsync(partnerId, ct) is not { } partner)
            return page;

        var segments = new List<PageSegment> { new SponsorDisclosureSegment(partner) };
        segments.AddRange(page.Segments);

        if (!reader.HasPremium)
        {
            var partnerAd = (await ads.PickAsync(null, 1, partner.Id, ct)).FirstOrDefault();
            segments.Add(new PartnerPromoSegment(partner, partnerAd));
        }

        return page with { Segments = segments };
    }
}
