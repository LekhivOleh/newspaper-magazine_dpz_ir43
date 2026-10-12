using CURSOVA.Application.Abstractions.Persistence;
using CURSOVA.Domain.Issues;
using Microsoft.EntityFrameworkCore;

namespace CURSOVA.Infrastructure.Persistence.Repositories;

// Loading all components of an issue in one query lets EF stitch Parent/Children together,
// so the whole composite tree is available without recursive includes.
public class IssueRepository(MagazineDbContext db) : Repository<Issue>(db), IIssueRepository
{
    private IQueryable<Issue> WithTree => Set.Include(i => i.Components);

    public Task<Issue?> GetWithTreeAsync(int id, CancellationToken ct = default) =>
        WithTree.FirstOrDefaultAsync(i => i.Id == id, ct);

    public Task<Issue?> GetBySlugWithTreeAsync(string slug, CancellationToken ct = default) =>
        WithTree.FirstOrDefaultAsync(i => i.Slug == slug, ct);

    public async Task<IReadOnlyList<Issue>> ListWithTreeAsync(bool publishedOnly, CancellationToken ct = default) =>
        await WithTree
            .Where(i => !publishedOnly || i.IsPublished)
            .OrderByDescending(i => i.PublishedAt ?? i.CreatedAt)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Issue>> GetFeaturedAsync(CancellationToken ct = default) =>
        await WithTree.Where(i => i.IsFeatured).ToListAsync(ct);

    public Task<bool> SlugExistsAsync(string slug, int? excludeId = null, CancellationToken ct = default) =>
        Set.AnyAsync(i => i.Slug == slug && i.Id != excludeId, ct);
}
