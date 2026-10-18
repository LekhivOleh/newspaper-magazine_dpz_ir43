using CURSOVA.Application.Recommendations;

namespace CURSOVA.Application.Abstractions.Persistence;

public interface IRecommendationDataSource
{
    Task<IReadOnlyList<ArticleCandidate>> GetCandidatesAsync(int max, CancellationToken ct = default);
    Task<InterestSignals> GetSignalsAsync(int userId, CancellationToken ct = default);

    /// <summary>Articles the reader already keeps (saved or collected) — no point recommending them.</summary>
    Task<IReadOnlySet<int>> GetKeptArticleIdsAsync(int userId, CancellationToken ct = default);

    Task<IReadOnlyList<int>> GetExplicitInterestsAsync(int userId, CancellationToken ct = default);
    Task SetExplicitInterestsAsync(int userId, IReadOnlyCollection<int> categoryIds, CancellationToken ct = default);
}
