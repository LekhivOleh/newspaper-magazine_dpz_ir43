using CURSOVA.Application.Abstractions.Persistence;
using CURSOVA.Domain.Categories;
using Microsoft.EntityFrameworkCore;

namespace CURSOVA.Infrastructure.Persistence.Repositories;

public class CategoryRepository(MagazineDbContext db) : Repository<Category>(db), ICategoryRepository
{
    public override async Task<IReadOnlyList<Category>> ListAsync(CancellationToken ct = default) =>
        await Set.AsNoTracking().OrderBy(c => c.SortOrder).ThenBy(c => c.Name).ToListAsync(ct);

    public Task<Category?> GetBySlugAsync(string slug, CancellationToken ct = default) =>
        Set.FirstOrDefaultAsync(c => c.Slug == slug, ct);

    public Task<bool> SlugExistsAsync(string slug, int? excludeId = null, CancellationToken ct = default) =>
        Set.AnyAsync(c => c.Slug == slug && c.Id != excludeId, ct);

    public Task<bool> HasArticlesAsync(int categoryId, CancellationToken ct = default) =>
        Db.Articles.AnyAsync(a => a.CategoryId == categoryId, ct);
}
