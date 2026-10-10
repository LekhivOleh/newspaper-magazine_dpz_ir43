using CURSOVA.Domain.Categories;

namespace CURSOVA.Application.Abstractions.Persistence;

public interface ICategoryRepository : IRepository<Category>
{
    Task<Category?> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task<bool> SlugExistsAsync(string slug, int? excludeId = null, CancellationToken ct = default);
    Task<bool> HasArticlesAsync(int categoryId, CancellationToken ct = default);
}
