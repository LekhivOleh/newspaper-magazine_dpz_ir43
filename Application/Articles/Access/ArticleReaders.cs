using CURSOVA.Application.Abstractions.Identity;
using CURSOVA.Application.Subscriptions;
using CURSOVA.Domain.Articles;

namespace CURSOVA.Application.Articles.Access;

/// <summary>Proxy subject: how readers (pages and the public API) obtain article content.</summary>
public interface IArticleReader
{
    Task<ArticleDetailsDto> GetAsync(int id, CancellationToken ct = default);
    Task<ArticleDetailsDto> GetBySlugAsync(string slug, CancellationToken ct = default);
}

/// <summary>Real subject: returns the article exactly as stored, with no access rules.</summary>
public class ArticleReader(IArticleService articles) : IArticleReader
{
    public Task<ArticleDetailsDto> GetAsync(int id, CancellationToken ct = default) => articles.GetAsync(id, ct);

    public Task<ArticleDetailsDto> GetBySlugAsync(string slug, CancellationToken ct = default) => articles.GetBySlugAsync(slug, ct);
}

/// <summary>
/// Protection proxy: same interface as the real reader, but checks the reader's subscription first.
/// Without Premium, a Premium article is cut down to a teaser and exclusive media is replaced by placeholders.
/// Callers can't tell whether they talk to the proxy or the real reader.
/// </summary>
public class PremiumContentProxy(IArticleReader inner, ISubscriptionService subscriptions, ICurrentUser currentUser) : IArticleReader
{
    public const int TeaserBlocks = 2;

    public async Task<ArticleDetailsDto> GetAsync(int id, CancellationToken ct = default) =>
        await ProtectAsync(await inner.GetAsync(id, ct), ct);

    public async Task<ArticleDetailsDto> GetBySlugAsync(string slug, CancellationToken ct = default) =>
        await ProtectAsync(await inner.GetBySlugAsync(slug, ct), ct);

    private async Task<ArticleDetailsDto> ProtectAsync(ArticleDetailsDto article, CancellationToken ct)
    {
        var isPremiumArticle = article.Summary.AccessLevel == AccessLevel.Premium;
        var hasExclusiveMedia = article.Blocks.Any(b => b.IsExclusive);
        if (!isPremiumArticle && !hasExclusiveMedia)
            return article with { Access = ContentAccessDto.Unrestricted };

        // Editors must always see what they publish.
        var hasPremium = currentUser.IsAdmin || await subscriptions.HasPremiumAccessAsync(ct);
        if (hasPremium)
            return article with { Access = new ContentAccessDto(true, 0, true) };

        return isPremiumArticle ? Teaser(article) : LockExclusive(article);
    }

    private static ArticleDetailsDto Teaser(ArticleDetailsDto article)
    {
        var visible = article.Blocks.Where(b => !b.IsExclusive).Take(TeaserBlocks).ToList();
        return article with
        {
            Blocks = visible,
            Access = new ContentAccessDto(false, article.Blocks.Count - visible.Count, false)
        };
    }

    private static ArticleDetailsDto LockExclusive(ArticleDetailsDto article) => article with
    {
        Blocks = article.Blocks
            .Select(b => b.IsExclusive ? new LockedBlockDto(TypeName(b)) { IsExclusive = true } : b)
            .ToList(),
        Access = new ContentAccessDto(true, article.Blocks.Count(b => b.IsExclusive), false)
    };

    private static string TypeName(BlockDto block) => block switch
    {
        ImageBlockDto => "image",
        VideoBlockDto => "video",
        GalleryBlockDto => "gallery",
        _ => "text"
    };
}
