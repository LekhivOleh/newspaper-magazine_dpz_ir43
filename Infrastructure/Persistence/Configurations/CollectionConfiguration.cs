using CURSOVA.Domain.Articles;
using CURSOVA.Domain.Profiles;
using CURSOVA.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CURSOVA.Infrastructure.Persistence.Configurations;

public class CollectionConfiguration : IEntityTypeConfiguration<Collection>
{
    public void Configure(EntityTypeBuilder<Collection> builder)
    {
        builder.Property(c => c.Name).HasMaxLength(Collection.NameMaxLength).IsRequired();
        builder.Property(c => c.OwnerName).HasMaxLength(100).IsRequired();
        builder.Property(c => c.Description).HasMaxLength(500);

        builder.HasOne<ApplicationUser>().WithMany().HasForeignKey(c => c.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Collection>().WithMany().HasForeignKey(c => c.CopiedFromId).OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(c => c.Items).WithOne().HasForeignKey(i => i.CollectionId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(c => c.Items).HasField("_items").UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(c => new { c.UserId, c.CreatedAt });
        builder.HasIndex(c => new { c.IsPublic, c.CreatedAt });
    }
}

public class CollectionItemConfiguration : IEntityTypeConfiguration<CollectionItem>
{
    public void Configure(EntityTypeBuilder<CollectionItem> builder)
    {
        builder.ToTable("CollectionItems");
        builder.Property(i => i.Note).HasMaxLength(300);
        builder.HasOne<Article>().WithMany().HasForeignKey(i => i.ArticleId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(i => new { i.CollectionId, i.ArticleId }).IsUnique();
    }
}
