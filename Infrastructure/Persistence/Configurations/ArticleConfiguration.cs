using CURSOVA.Domain.Articles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CURSOVA.Infrastructure.Persistence.Configurations;

public class ArticleConfiguration : IEntityTypeConfiguration<Article>
{
    public void Configure(EntityTypeBuilder<Article> builder)
    {
        builder.Property(a => a.Title).HasMaxLength(Article.TitleMaxLength).IsRequired();
        builder.Property(a => a.Slug).HasMaxLength(Article.TitleMaxLength * 2).IsRequired();
        builder.Property(a => a.Lead).HasMaxLength(Article.LeadMaxLength).IsRequired();
        builder.Property(a => a.CoverImageUrl).HasMaxLength(500);
        builder.Property(a => a.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(a => a.AccessLevel).HasConversion<string>().HasMaxLength(20);

        builder.HasIndex(a => a.Slug).IsUnique();
        builder.HasIndex(a => new { a.Status, a.PublishedAt });

        builder.HasOne(a => a.Category).WithMany().HasForeignKey(a => a.CategoryId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(a => a.Author).WithMany().HasForeignKey(a => a.AuthorId).OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(a => a.Blocks).WithOne().HasForeignKey(b => b.ArticleId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(a => a.Blocks).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Ignore(a => a.IsPublished);
        builder.Ignore(a => a.IsPremium);
    }
}
