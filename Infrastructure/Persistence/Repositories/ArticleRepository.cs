using CURSOVA.Application.Abstractions.Persistence;
using CURSOVA.Application.Common;
using CURSOVA.Domain.Articles;
using Microsoft.EntityFrameworkCore;

namespace CURSOVA.Infrastructure.Persistence.Repositories;

public class ArticleRepository(MagazineDbContext db) : Repository<Article>(db), IArticleRepository
{
    private IQueryable<Article> WithContent => Set
        .Include(a => a.Category)
        .Include(a => a.Author)
        .Include(a => a.Blocks.OrderBy(b => b.Position));

    public Task<Article?> GetWithContentAsync(int id, CancellationToken ct = default) =>
        WithContent.FirstOrDefaultAsync(a => a.Id == id, ct);

    public Task<Article?> GetBySlugWithContentAsync(string slug, CancellationToken ct = default) =>
        WithContent.FirstOrDefaultAsync(a => a.Slug == slug, ct);

    public Task<bool> SlugExistsAsync(string slug, int? excludeId = null, CancellationToken ct = default) =>
        Set.AnyAsync(a => a.Slug == slug && a.Id != excludeId, ct);

    public async Task<PagedResult<Article>> SearchAsync(ArticleQuery query, CancellationToken ct = default)
    {
        var articles = Set.AsNoTracking()
            .Include(a => a.Category)
            .Include(a => a.Author)
            .AsQueryable();

        if (query.Status is { } status)
            articles = articles.Where(a => a.Status == status);
        if (query.CategoryId is { } categoryId)
            articles = articles.Where(a => a.CategoryId == categoryId);
        if (query.AuthorId is { } authorId)
            articles = articles.Where(a => a.AuthorId == authorId);
        if (query.AccessLevel is { } accessLevel)
            articles = articles.Where(a => a.AccessLevel == accessLevel);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim().ToLowerInvariant();
            articles = articles.Where(a => a.SearchText.Contains(term));
        }

        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 50);

        var total = await articles.CountAsync(ct);
        var items = await articles
            .OrderByDescending(a => a.PublishedAt ?? a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<Article>(items, page, pageSize, total);
    }
}
