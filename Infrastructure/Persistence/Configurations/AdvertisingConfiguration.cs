using CURSOVA.Domain.Advertising;
using CURSOVA.Domain.Articles;
using CURSOVA.Domain.Categories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CURSOVA.Infrastructure.Persistence.Configurations;

public class PartnerConfiguration : IEntityTypeConfiguration<Partner>
{
    public void Configure(EntityTypeBuilder<Partner> builder)
    {
        builder.Property(p => p.Name).HasMaxLength(100).IsRequired();
        builder.Property(p => p.Tagline).HasMaxLength(200);
        builder.Property(p => p.LogoUrl).HasMaxLength(500);
        builder.Property(p => p.WebsiteUrl).HasMaxLength(500).IsRequired();

        builder.HasMany<Article>().WithOne().HasForeignKey(a => a.SponsorPartnerId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class AdConfiguration : IEntityTypeConfiguration<Ad>
{
    public void Configure(EntityTypeBuilder<Ad> builder)
    {
        builder.Property(a => a.Headline).HasMaxLength(120).IsRequired();
        builder.Property(a => a.Body).HasMaxLength(300);
        builder.Property(a => a.ImageUrl).HasMaxLength(500);
        builder.Property(a => a.TargetUrl).HasMaxLength(500).IsRequired();
        builder.Property(a => a.CallToAction).HasMaxLength(40).IsRequired();
        builder.Ignore(a => a.ClickThroughRate);

        builder.HasOne<Partner>().WithMany().HasForeignKey(a => a.PartnerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Category>().WithMany().HasForeignKey(a => a.CategoryId).OnDelete(DeleteBehavior.SetNull);
        builder.HasIndex(a => a.IsActive);
    }
}
