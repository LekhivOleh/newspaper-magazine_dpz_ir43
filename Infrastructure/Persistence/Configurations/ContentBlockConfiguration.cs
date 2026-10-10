using CURSOVA.Domain.Articles.Blocks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CURSOVA.Infrastructure.Persistence.Configurations;

public class ContentBlockConfiguration : IEntityTypeConfiguration<ContentBlock>
{
    public void Configure(EntityTypeBuilder<ContentBlock> builder)
    {
        builder.ToTable("ContentBlocks");
        builder.Ignore(b => b.Type);

        builder.HasDiscriminator<string>("Kind")
            .HasValue<TextBlock>("text")
            .HasValue<ImageBlock>("image")
            .HasValue<VideoBlock>("video")
            .HasValue<GalleryBlock>("gallery");
        builder.Property<string>("Kind").HasMaxLength(20);

        builder.HasIndex(b => new { b.ArticleId, b.Position });
    }
}

public class TextBlockConfiguration : IEntityTypeConfiguration<TextBlock>
{
    public void Configure(EntityTypeBuilder<TextBlock> builder)
    {
        builder.Property(b => b.Heading).HasMaxLength(200);
    }
}

public class ImageBlockConfiguration : IEntityTypeConfiguration<ImageBlock>
{
    public void Configure(EntityTypeBuilder<ImageBlock> builder)
    {
        builder.Property(b => b.Url).HasColumnName("Url").HasMaxLength(500);
        builder.Property(b => b.Caption).HasColumnName("Caption").HasMaxLength(300);
    }
}

public class VideoBlockConfiguration : IEntityTypeConfiguration<VideoBlock>
{
    public void Configure(EntityTypeBuilder<VideoBlock> builder)
    {
        builder.Property(b => b.EmbedUrl).HasColumnName("Url").HasMaxLength(500);
        builder.Property(b => b.Caption).HasColumnName("Caption").HasMaxLength(300);
    }
}

public class GalleryBlockConfiguration : IEntityTypeConfiguration<GalleryBlock>
{
    public void Configure(EntityTypeBuilder<GalleryBlock> builder)
    {
        builder.Property(b => b.Title).HasMaxLength(200);
        builder.OwnsMany(b => b.Images, images => images.ToJson());
        builder.Navigation(b => b.Images).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
