using CURSOVA.Domain.Authors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CURSOVA.Infrastructure.Persistence.Configurations;

public class AuthorConfiguration : IEntityTypeConfiguration<Author>
{
    public void Configure(EntityTypeBuilder<Author> builder)
    {
        builder.Property(a => a.FullName).HasMaxLength(120).IsRequired();
        builder.Property(a => a.Position).HasMaxLength(80);
        builder.Property(a => a.Bio).HasMaxLength(1000);
        builder.Property(a => a.AvatarUrl).HasMaxLength(500);
    }
}
