using CURSOVA.Domain.Articles;
using CURSOVA.Domain.Comments;
using CURSOVA.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CURSOVA.Infrastructure.Persistence.Configurations;

public class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        builder.Property(c => c.AuthorName).HasMaxLength(100).IsRequired();
        builder.Property(c => c.Body).HasMaxLength(Comment.BodyMaxLength).IsRequired();
        builder.Property(c => c.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(c => c.ModerationNote).HasMaxLength(300);

        builder.HasOne<Article>().WithMany().HasForeignKey(c => c.ArticleId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<ApplicationUser>().WithMany().HasForeignKey(c => c.UserId).OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(c => new { c.ArticleId, c.Status, c.CreatedAt });
        builder.HasIndex(c => new { c.UserId, c.CreatedAt });
    }
}
