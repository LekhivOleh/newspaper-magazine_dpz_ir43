using CURSOVA.Application.Common;
using CURSOVA.Domain.Articles;

namespace CURSOVA.Application.Abstractions.Persistence;

public interface IArticleRepository : IRepository<Article>
{
    Task<Article?> GetWithContentAsync(int id, CancellationToken ct = default);
    Task<Article?> GetBySlugWithContentAsync(string slug, CancellationToken ct = default);
    Task<bool> SlugExistsAsync(string slug, int? excludeId = null, CancellationToken ct = default);
    Task<PagedResult<Article>> SearchAsync(ArticleQuery query, CancellationToken ct = default);
    Task<IReadOnlyList<Article>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default);
}

public record ArticleQuery
{
    public int? CategoryId { get; init; }
    public int? AuthorId { get; init; }
    public AccessLevel? AccessLevel { get; init; }
    public ArticleStatus? Status { get; init; } = ArticleStatus.Published;
    public string? Search { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}
