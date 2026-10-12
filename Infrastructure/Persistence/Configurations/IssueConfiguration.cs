using CURSOVA.Domain.Articles;
using CURSOVA.Domain.Issues;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CURSOVA.Infrastructure.Persistence.Configurations;

public class IssueConfiguration : IEntityTypeConfiguration<Issue>
{
    public void Configure(EntityTypeBuilder<Issue> builder)
    {
        builder.Property(i => i.Title).HasMaxLength(Issue.TitleMaxLength).IsRequired();
        builder.Property(i => i.Slug).HasMaxLength(Issue.TitleMaxLength * 2).IsRequired();
        builder.Property(i => i.Description).HasMaxLength(Issue.DescriptionMaxLength).IsRequired();
        builder.Property(i => i.CoverImageUrl).HasMaxLength(500);
        builder.HasIndex(i => i.Slug).IsUnique();

        builder.HasMany(i => i.Components).WithOne().HasForeignKey(c => c.IssueId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(i => i.Components).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Ignore(i => i.Root);
    }
}

public class IssueComponentConfiguration : IEntityTypeConfiguration<IssueComponent>
{
    public void Configure(EntityTypeBuilder<IssueComponent> builder)
    {
        builder.ToTable("IssueComponents");
        builder.HasDiscriminator<string>("Kind")
            .HasValue<IssueSection>("section")
            .HasValue<ArticleItem>("article");
        builder.Property<string>("Kind").HasMaxLength(20);

        builder.HasIndex(c => new { c.IssueId, c.ParentId, c.Position });
    }
}

public class IssueSectionConfiguration : IEntityTypeConfiguration<IssueSection>
{
    public void Configure(EntityTypeBuilder<IssueSection> builder)
    {
        builder.Property(s => s.Title).HasMaxLength(IssueSection.TitleMaxLength);

        builder.HasMany(s => s.Children).WithOne(c => c.Parent).HasForeignKey(c => c.ParentId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(s => s.Children).HasField("_children").UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

public class ArticleItemConfiguration : IEntityTypeConfiguration<ArticleItem>
{
    public void Configure(EntityTypeBuilder<ArticleItem> builder)
    {
        builder.Property(a => a.EditorNote).HasMaxLength(300);
        builder.HasOne<Article>().WithMany().HasForeignKey(a => a.ArticleId).OnDelete(DeleteBehavior.Cascade);
    }
}
