using CURSOVA.Domain.Sharing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CURSOVA.Infrastructure.Persistence.Configurations;

public class ShareEventConfiguration : IEntityTypeConfiguration<ShareEvent>
{
    public void Configure(EntityTypeBuilder<ShareEvent> builder)
    {
        builder.Property(s => s.ContentType).HasConversion<string>().HasMaxLength(20);
        builder.Property(s => s.Network).HasMaxLength(30).IsRequired();
        builder.HasIndex(s => new { s.ContentType, s.ContentId, s.Network });
    }
}
