using CURSOVA.Application.Abstractions.Persistence;
using CURSOVA.Application.Abstractions.Sharing;
using CURSOVA.Application.Articles;
using CURSOVA.Application.Common;
using CURSOVA.Application.Interactive;
using CURSOVA.Domain.Articles;
using CURSOVA.Domain.Common;
using CURSOVA.Domain.Sharing;

namespace CURSOVA.Application.Sharing;

/// <param name="Url">Tracked link through our server (/share/...), used by the UI.</param>
/// <param name="DirectUrl">The network's own share URL.</param>
public record ShareLinkDto(string Network, string DisplayName, string Url, string DirectUrl);

public record ShareStatsDto(int Total, IReadOnlyDictionary<string, int> ByNetwork);

public record TopSharedArticleDto(ArticleSummaryDto Article, int Shares);

public interface ISharingFacade
{
    Task<IReadOnlyList<ShareLinkDto>> GetLinksAsync(ShareableType type, int id, CancellationToken ct = default);
    Task<string> ShareAsync(ShareableType type, int id, string network, CancellationToken ct = default);
    Task<ShareStatsDto> GetStatsAsync(ShareableType type, int id, CancellationToken ct = default);
    Task<IReadOnlyList<TopSharedArticleDto>> GetTopArticlesAsync(int take, CancellationToken ct = default);
}

/// <summary>
/// Facade over the sharing subsystem: content lookup, absolute URLs, hashtags,
/// per-network adapters and share tracking stay hidden behind three simple calls.
/// </summary>
public class SharingFacade(
    IEnumerable<IShareTarget> targets,
    IArticleService articles,
    IQuizService quizzes,
    IAppUrlProvider urls,
    IUnitOfWork uow) : ISharingFacade
{
    private const string BrandHashtag = "TechPulse";

    private readonly IReadOnlyList<IShareTarget> _targets = targets.ToList();

    public async Task<IReadOnlyList<ShareLinkDto>> GetLinksAsync(ShareableType type, int id, CancellationToken ct = default)
    {
        var content = await BuildContentAsync(type, id, ct);
        return _targets
            .Select(t => new ShareLinkDto(t.Key, t.DisplayName, TrackedUrl(type, id, t.Key), t.BuildShareUrl(content)))
            .ToList();
    }

    public async Task<string> ShareAsync(ShareableType type, int id, string network, CancellationToken ct = default)
    {
        var target = _targets.FirstOrDefault(t => t.Key == network)
                     ?? throw new DomainException($"Невідома соцмережа «{network}».");
        var content = await BuildContentAsync(type, id, ct);

        uow.Shares.Add(new ShareEvent(type, id, target.Key));
        await uow.SaveChangesAsync(ct);

        return target.BuildShareUrl(content);
    }

    public async Task<ShareStatsDto> GetStatsAsync(ShareableType type, int id, CancellationToken ct = default)
    {
        var byNetwork = await uow.Shares.CountByNetworkAsync(type, id, ct);
        return new ShareStatsDto(byNetwork.Values.Sum(), byNetwork);
    }

    public async Task<IReadOnlyList<TopSharedArticleDto>> GetTopArticlesAsync(int take, CancellationToken ct = default)
    {
        var result = new List<TopSharedArticleDto>();
        foreach (var (articleId, count) in await uow.Shares.TopAsync(ShareableType.Article, take, ct))
        {
            try
            {
                result.Add(new TopSharedArticleDto((await articles.GetAsync(articleId, ct)).Summary, count));
            }
            catch (NotFoundException)
            {
                // Article deleted since it was shared.
            }
        }
        return result;
    }

    private async Task<ShareContent> BuildContentAsync(ShareableType type, int id, CancellationToken ct)
    {
        switch (type)
        {
            case ShareableType.Article:
            {
                var article = (await articles.GetAsync(id, ct)).Summary;
                if (article.Status != ArticleStatus.Published)
                    throw NotFoundException.For<Article>(id);

                return new ShareContent(urls.ArticleUrl(article.Slug), article.Title, article.Lead,
                    [BrandHashtag, ToHashtag(article.Category.Name)]);
            }
            case ShareableType.Quiz:
            {
                var quiz = (await quizzes.GetAsync(id, ct)).Summary;
                var mine = await quizzes.GetMyLastResultAsync(id, ct);
                var title = mine is null
                    ? $"Тест «{quiz.Title}»"
                    : $"Мій результат у тесті «{quiz.Title}» — {mine.OutcomeTitle}! А який у тебе?";

                return new ShareContent(urls.QuizUrl(id), title, quiz.Description, [BrandHashtag, "тест"]);
            }
            default:
                throw new DomainException("Цей тип контенту не можна поширити.");
        }
    }

    private static string TrackedUrl(ShareableType type, int id, string network) =>
        $"/share/{type.ToString().ToLowerInvariant()}/{id}/{network}";

    private static string ToHashtag(string text) =>
        string.Concat(text.Replace("'", "").Replace("’", "")
            .Split(' ', '-').Where(w => w.Length > 0).Select(w => char.ToUpperInvariant(w[0]) + w[1..]));
}
