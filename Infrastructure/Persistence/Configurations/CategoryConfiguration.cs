using CURSOVA.Domain.Categories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CURSOVA.Infrastructure.Persistence.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.Property(c => c.Name).HasMaxLength(Category.NameMaxLength).IsRequired();
        builder.Property(c => c.Slug).HasMaxLength(Category.NameMaxLength * 2).IsRequired();
        builder.Property(c => c.Description).HasMaxLength(Category.DescriptionMaxLength);

        builder.HasIndex(c => c.Slug).IsUnique();
    }
}
