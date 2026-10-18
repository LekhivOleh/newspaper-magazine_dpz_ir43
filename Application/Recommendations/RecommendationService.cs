using CURSOVA.Application.Abstractions.Identity;
using CURSOVA.Application.Abstractions.Persistence;
using CURSOVA.Application.Articles;
using CURSOVA.Application.Categories;
using CURSOVA.Domain.Common;

namespace CURSOVA.Application.Recommendations;

public record RecommendedArticleDto(ArticleSummaryDto Article, string Reason);

public record RecommendationsDto(string StrategyKey, string StrategyTitle, IReadOnlyList<RecommendedArticleDto> Items);

public record InterestDto(CategoryDto Category, bool Selected, double Weight);

public interface IRecommendationService
{
    IReadOnlyList<(string Key, string Title)> Strategies { get; }
    Task<RecommendationsDto> RecommendAsync(int take, string? strategyKey = null, CancellationToken ct = default);
    Task<IReadOnlyList<InterestDto>> GetInterestsAsync(CancellationToken ct = default);
    Task SetInterestsAsync(IReadOnlyCollection<int> categoryIds, CancellationToken ct = default);
}

public class RecommendationService(
    IEnumerable<IRecommendationStrategy> strategies,
    IRecommendationDataSource data,
    ICategoryService categories,
    IUnitOfWork uow,
    ICurrentUser currentUser) : IRecommendationService
{
    private const int CandidatePoolSize = 200;

    private readonly IReadOnlyList<IRecommendationStrategy> _strategies = strategies.ToList();

    public IReadOnlyList<(string Key, string Title)> Strategies => _strategies.Select(s => (s.Key, s.Title)).ToList();

    public async Task<RecommendationsDto> RecommendAsync(int take, string? strategyKey = null, CancellationToken ct = default)
    {
        var interests = InterestProfile.Empty;
        IReadOnlySet<int> kept = new HashSet<int>();
        if (currentUser.Id is { } userId)
        {
            interests = (await data.GetSignalsAsync(userId, ct)).ToProfile();
            kept = await data.GetKeptArticleIdsAsync(userId, ct);
        }

        var strategy = Choose(strategyKey, interests);
        var candidates = await data.GetCandidatesAsync(CandidatePoolSize, ct);
        var picked = strategy.Recommend(new RecommendationContext(interests, kept, DateTime.UtcNow), candidates, Math.Clamp(take, 1, 24));

        var articles = (await uow.Articles.GetByIdsAsync(picked.Select(p => p.ArticleId), ct)).ToDictionary(a => a.Id, a => a.ToSummary());
        return new RecommendationsDto(strategy.Key, strategy.Title,
            picked.Where(p => articles.ContainsKey(p.ArticleId))
                .Select(p => new RecommendedArticleDto(articles[p.ArticleId], p.Reason))
                .ToList());
    }

    public async Task<IReadOnlyList<InterestDto>> GetInterestsAsync(CancellationToken ct = default)
    {
        var userId = RequireUser();
        var signals = await data.GetSignalsAsync(userId, ct);
        var profile = signals.ToProfile();

        return (await categories.ListAsync(ct))
            .Select(c => new InterestDto(c, signals.ExplicitCategoryIds.Contains(c.Id), Math.Round(profile.WeightOf(c.Id), 2)))
            .ToList();
    }

    public async Task SetInterestsAsync(IReadOnlyCollection<int> categoryIds, CancellationToken ct = default)
    {
        var known = (await categories.ListAsync(ct)).Select(c => c.Id).ToHashSet();
        if (categoryIds.Any(id => !known.Contains(id)))
            throw new DomainException("Невідома рубрика.");

        await data.SetExplicitInterestsAsync(RequireUser(), categoryIds, ct);
    }

    /// <summary>Explicit choice wins; otherwise readers we know nothing about get what's popular.</summary>
    private IRecommendationStrategy Choose(string? key, InterestProfile interests)
    {
        if (!string.IsNullOrWhiteSpace(key))
            return _strategies.FirstOrDefault(s => s.Key == key) ?? throw new DomainException($"Невідома стратегія «{key}».");

        return _strategies.First(s => s.Key == (interests.IsEmpty ? "popular" : "hybrid"));
    }

    private int RequireUser() => currentUser.Id ?? throw new DomainException("Увійдіть, щоб налаштувати інтереси.");
}
