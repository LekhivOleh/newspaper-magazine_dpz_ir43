using System.ComponentModel.DataAnnotations;
using CURSOVA.Domain.Categories;

namespace CURSOVA.Application.Categories;

public record CategoryDto(int Id, string Name, string Slug, string? Description, int SortOrder)
{
    public static CategoryDto From(Category category) =>
        new(category.Id, category.Name, category.Slug, category.Description, category.SortOrder);
}

public record SaveCategoryRequest(
    [Required, MaxLength(Category.NameMaxLength)] string Name,
    [MaxLength(Category.DescriptionMaxLength)] string? Description,
    int SortOrder = 0);
