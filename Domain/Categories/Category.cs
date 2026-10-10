using CURSOVA.Domain.Common;

namespace CURSOVA.Domain.Categories;

public class Category : Entity
{
    public const int NameMaxLength = 80;
    public const int DescriptionMaxLength = 500;

    public string Name { get; private set; } = null!;
    public string Slug { get; private set; } = null!;
    public string? Description { get; private set; }
    public int SortOrder { get; private set; }

    private Category() { }

    public Category(string name, string? description = null, int sortOrder = 0)
    {
        Update(name, description, sortOrder);
    }

    public void Update(string name, string? description, int sortOrder)
    {
        Name = Guard.NotEmpty(name, "Назва рубрики", NameMaxLength);
        Slug = Common.Slug.From(Name);
        Description = Guard.Optional(description, "Опис рубрики", DescriptionMaxLength);
        SortOrder = sortOrder;
    }
}
