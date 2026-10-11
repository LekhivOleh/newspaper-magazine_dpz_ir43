using CURSOVA.Application.Abstractions.Persistence;
using CURSOVA.Application.Common;
using CURSOVA.Domain.Categories;

namespace CURSOVA.Application.Categories;

public class CategoryService(IUnitOfWork uow) : ICategoryService
{
    public async Task<IReadOnlyList<CategoryDto>> ListAsync(CancellationToken ct = default) =>
        (await uow.Categories.ListAsync(ct)).Select(CategoryDto.From).ToList();

    public async Task<CategoryDto> GetAsync(int id, CancellationToken ct = default) =>
        CategoryDto.From(await LoadAsync(id, ct));

    public async Task<CategoryDto> GetBySlugAsync(string slug, CancellationToken ct = default)
    {
        var category = await uow.Categories.GetBySlugAsync(slug, ct)
                       ?? throw NotFoundException.For<Category>(slug);
        return CategoryDto.From(category);
    }

    public async Task<CategoryDto> CreateAsync(SaveCategoryRequest request, CancellationToken ct = default)
    {
        var category = new Category(request.Name, request.Description, request.SortOrder);
        await EnsureUniqueSlugAsync(category, ct);

        uow.Categories.Add(category);
        await uow.SaveChangesAsync(ct);
        return CategoryDto.From(category);
    }

    public async Task<CategoryDto> UpdateAsync(int id, SaveCategoryRequest request, CancellationToken ct = default)
    {
        var category = await LoadAsync(id, ct);
        category.Update(request.Name, request.Description, request.SortOrder);
        await EnsureUniqueSlugAsync(category, ct);

        await uow.SaveChangesAsync(ct);
        return CategoryDto.From(category);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var category = await LoadAsync(id, ct);
        if (await uow.Categories.HasArticlesAsync(id, ct))
            throw new ConflictException("Не можна видалити рубрику, у якій є статті.");

        uow.Categories.Remove(category);
        await uow.SaveChangesAsync(ct);
    }

    private async Task<Category> LoadAsync(int id, CancellationToken ct) =>
        await uow.Categories.GetByIdAsync(id, ct) ?? throw NotFoundException.For<Category>(id);

    private async Task EnsureUniqueSlugAsync(Category category, CancellationToken ct)
    {
        var excludeId = category.Id == 0 ? (int?)null : category.Id;
        if (await uow.Categories.SlugExistsAsync(category.Slug, excludeId, ct))
            throw new ConflictException($"Рубрика з назвою «{category.Name}» уже існує.");
    }
}
