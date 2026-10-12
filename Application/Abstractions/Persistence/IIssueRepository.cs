using CURSOVA.Domain.Issues;

namespace CURSOVA.Application.Abstractions.Persistence;

public interface IIssueRepository : IRepository<Issue>
{
    Task<Issue?> GetWithTreeAsync(int id, CancellationToken ct = default);
    Task<Issue?> GetBySlugWithTreeAsync(string slug, CancellationToken ct = default);
    Task<IReadOnlyList<Issue>> ListWithTreeAsync(bool publishedOnly, CancellationToken ct = default);
    Task<IReadOnlyList<Issue>> GetFeaturedAsync(CancellationToken ct = default);
    Task<bool> SlugExistsAsync(string slug, int? excludeId = null, CancellationToken ct = default);
}
