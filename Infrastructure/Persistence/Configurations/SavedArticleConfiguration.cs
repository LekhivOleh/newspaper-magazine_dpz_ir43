using CURSOVA.Domain.Articles;
using CURSOVA.Domain.Profiles;
using CURSOVA.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CURSOVA.Infrastructure.Persistence.Configurations;

public class SavedArticleConfiguration : IEntityTypeConfiguration<SavedArticle>
{
    public void Configure(EntityTypeBuilder<SavedArticle> builder)
    {
        builder.HasIndex(s => new { s.UserId, s.ArticleId }).IsUnique();
        builder.HasOne<ApplicationUser>().WithMany().HasForeignKey(s => s.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Article>().WithMany().HasForeignKey(s => s.ArticleId).OnDelete(DeleteBehavior.Cascade);
    }
}
