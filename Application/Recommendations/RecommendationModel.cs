namespace CURSOVA.Application.Recommendations;

/// <summary>Everything a strategy may look at for one published article.</summary>
public record ArticleCandidate(
    int ArticleId,
    int CategoryId,
    string CategoryName,
    DateTime PublishedAt,
    int ViewCount,
    int ShareCount,
    int CommentCount);

/// <summary>Category → weight, built from what the reader did on the site.</summary>
public class InterestProfile(IReadOnlyDictionary<int, double> weights)
{
    public static readonly InterestProfile Empty = new(new Dictionary<int, double>());

    public IReadOnlyDictionary<int, double> Weights { get; } = weights;

    public bool IsEmpty => Weights.Count == 0;

    public double WeightOf(int categoryId) => Weights.GetValueOrDefault(categoryId);
}

/// <summary>Raw signals per category, counted from the database.</summary>
public record InterestSignals(
    IReadOnlyCollection<int> ExplicitCategoryIds,
    IReadOnlyDictionary<int, int> SavedByCategory,
    IReadOnlyDictionary<int, int> CollectedByCategory,
    IReadOnlyDictionary<int, int> CommentedByCategory)
{
    public const double ExplicitWeight = 5, SavedWeight = 3, CollectedWeight = 2, CommentedWeight = 1;

    public InterestProfile ToProfile()
    {
        var weights = new Dictionary<int, double>();

        void Add(int categoryId, double amount) => weights[categoryId] = weights.GetValueOrDefault(categoryId) + amount;

        foreach (var id in ExplicitCategoryIds) Add(id, ExplicitWeight);
        foreach (var (id, count) in SavedByCategory) Add(id, SavedWeight * count);
        foreach (var (id, count) in CollectedByCategory) Add(id, CollectedWeight * count);
        foreach (var (id, count) in CommentedByCategory) Add(id, CommentedWeight * count);

        var max = weights.Count == 0 ? 1 : weights.Values.Max();
        return new InterestProfile(weights.ToDictionary(w => w.Key, w => w.Value / max));
    }
}

public record RecommendationContext(InterestProfile Interests, IReadOnlySet<int> ExcludedArticleIds, DateTime Now);

public record ScoredArticle(int ArticleId, double Score, string Reason);
