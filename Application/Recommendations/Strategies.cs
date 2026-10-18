namespace CURSOVA.Application.Recommendations;

public interface IRecommendationStrategy
{
    string Key { get; }
    string Title { get; }

    IReadOnlyList<ScoredArticle> Recommend(RecommendationContext context, IReadOnlyList<ArticleCandidate> candidates, int take);
}

internal static class Freshness
{
    /// <summary>1.0 for today's article, 0.5 after a month, slowly fading after that.</summary>
    public static double Factor(DateTime publishedAt, DateTime now) =>
        1.0 / (1.0 + Math.Max(0, (now - publishedAt).TotalDays) / 30.0);
}

public class InterestBasedStrategy : IRecommendationStrategy
{
    public string Key => "interests";
    public string Title => "За вашими інтересами";

    public IReadOnlyList<ScoredArticle> Recommend(RecommendationContext context, IReadOnlyList<ArticleCandidate> candidates, int take) =>
        candidates
            .Where(c => !context.ExcludedArticleIds.Contains(c.ArticleId))
            .Select(c => (Candidate: c, Score: context.Interests.WeightOf(c.CategoryId) * Freshness.Factor(c.PublishedAt, context.Now)))
            .Where(x => x.Score > 0)
            .OrderByDescending(x => x.Score)
            .Take(take)
            .Select(x => new ScoredArticle(x.Candidate.ArticleId, x.Score, $"Бо ви цікавитесь «{x.Candidate.CategoryName}»"))
            .ToList();
}

public class PopularityStrategy : IRecommendationStrategy
{
    public string Key => "popular";
    public string Title => "Популярне зараз";

    public IReadOnlyList<ScoredArticle> Recommend(RecommendationContext context, IReadOnlyList<ArticleCandidate> candidates, int take) =>
        candidates
            .Where(c => !context.ExcludedArticleIds.Contains(c.ArticleId))
            .Select(c => (Candidate: c, Score: Popularity(c) * Freshness.Factor(c.PublishedAt, context.Now)))
            .OrderByDescending(x => x.Score)
            .ThenByDescending(x => x.Candidate.PublishedAt)
            .Take(take)
            .Select(x => new ScoredArticle(x.Candidate.ArticleId, x.Score, "Популярне серед читачів"))
            .ToList();

    // Logarithms keep one viral article from drowning out everything else; shares and comments mean more than a view.
    private static double Popularity(ArticleCandidate c) =>
        1 + Math.Log(1 + c.ViewCount) + 2 * Math.Log(1 + c.ShareCount) + 1.5 * Math.Log(1 + c.CommentCount);
}

/// <summary>
/// Blends several strategies: each one's scores are normalised to 0..1 and weighted.
/// The reason shown is taken from the strategy that contributed most to the article.
/// </summary>
public class HybridStrategy(params (IRecommendationStrategy Strategy, double Weight)[] parts) : IRecommendationStrategy
{
    public string Key => "hybrid";
    public string Title => "Рекомендовано для вас";

    public IReadOnlyList<ScoredArticle> Recommend(RecommendationContext context, IReadOnlyList<ArticleCandidate> candidates, int take)
    {
        var totals = new Dictionary<int, (double Score, double BestContribution, string Reason)>();

        foreach (var (strategy, weight) in parts)
        {
            var scored = strategy.Recommend(context, candidates, candidates.Count);
            var max = scored.Count == 0 ? 1 : scored.Max(s => s.Score);

            foreach (var item in scored)
            {
                var contribution = weight * item.Score / max;
                var current = totals.GetValueOrDefault(item.ArticleId);
                totals[item.ArticleId] = contribution > current.BestContribution
                    ? (current.Score + contribution, contribution, item.Reason)
                    : (current.Score + contribution, current.BestContribution, current.Reason);
            }
        }

        return totals
            .OrderByDescending(t => t.Value.Score)
            .Take(take)
            .Select(t => new ScoredArticle(t.Key, t.Value.Score, t.Value.Reason))
            .ToList();
    }
}
