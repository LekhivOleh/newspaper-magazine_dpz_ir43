using CURSOVA.Application.Recommendations;
using static CURSOVA.Tests.TestSupport;

namespace CURSOVA.Tests.Application;

public class RecommendationStrategyTests
{
    private const int Gadgets = 1, Games = 2;

    private static readonly ArticleCandidate[] Candidates =
    [
        new(1, Gadgets, "Гаджети", Now.AddDays(-1), ViewCount: 10, ShareCount: 0, CommentCount: 0),
        new(2, Gadgets, "Гаджети", Now.AddDays(-60), 10, 0, 0),
        new(3, Games, "Ігри", Now.AddDays(-1), ViewCount: 5000, ShareCount: 50, CommentCount: 40),
        new(4, Games, "Ігри", Now.AddDays(-2), 0, 0, 0)
    ];

    private static RecommendationContext Context(IReadOnlyDictionary<int, double>? interests = null, params int[] excluded) =>
        new(new InterestProfile(interests ?? new Dictionary<int, double>()), excluded.ToHashSet(), Now);

    [Fact]
    public void Interest_strategy_recommends_only_liked_categories_fresh_first()
    {
        var result = new InterestBasedStrategy().Recommend(Context(new Dictionary<int, double> { [Gadgets] = 1 }), Candidates, 10);

        Assert.Equal([1, 2], result.Select(r => r.ArticleId));
        Assert.Contains("Гаджети", result[0].Reason);
    }

    [Fact]
    public void Interest_strategy_returns_nothing_for_an_empty_profile()
    {
        Assert.Empty(new InterestBasedStrategy().Recommend(Context(), Candidates, 10));
    }

    [Fact]
    public void Popularity_strategy_favours_engagement()
    {
        var result = new PopularityStrategy().Recommend(Context(), Candidates, 2);

        Assert.Equal(3, result[0].ArticleId);
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void Already_read_or_saved_articles_are_excluded_by_every_strategy()
    {
        var context = Context(new Dictionary<int, double> { [Gadgets] = 1, [Games] = 1 }, excluded: [1, 3]);
        IRecommendationStrategy[] strategies =
        [
            new InterestBasedStrategy(),
            new PopularityStrategy(),
            new HybridStrategy((new InterestBasedStrategy(), 0.7), (new PopularityStrategy(), 0.3))
        ];

        foreach (var strategy in strategies)
            Assert.DoesNotContain(strategy.Recommend(context, Candidates, 10), r => r.ArticleId is 1 or 3);
    }

    [Fact]
    public void Hybrid_blends_strategies_by_weight()
    {
        var context = Context(new Dictionary<int, double> { [Gadgets] = 1 });
        var interestHeavy = new HybridStrategy((new InterestBasedStrategy(), 0.9), (new PopularityStrategy(), 0.1));
        var popularityHeavy = new HybridStrategy((new InterestBasedStrategy(), 0.1), (new PopularityStrategy(), 0.9));

        Assert.Equal(1, interestHeavy.Recommend(context, Candidates, 1)[0].ArticleId);
        Assert.Equal(3, popularityHeavy.Recommend(context, Candidates, 1)[0].ArticleId);
    }

    [Fact]
    public void Interest_profile_weighs_signals_and_normalises_to_one()
    {
        var profile = new InterestSignals(
            ExplicitCategoryIds: [Gadgets],
            SavedByCategory: new Dictionary<int, int> { [Games] = 1 },
            CollectedByCategory: new Dictionary<int, int>(),
            CommentedByCategory: new Dictionary<int, int> { [Gadgets] = 1 }).ToProfile();

        Assert.Equal(1.0, profile.WeightOf(Gadgets));
        Assert.Equal(0.5, profile.WeightOf(Games));
        Assert.Equal(0, profile.WeightOf(99));
    }
}
