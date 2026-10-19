using CURSOVA.Domain.Articles;
using CURSOVA.Domain.Articles.Blocks;

namespace CURSOVA.Application.Articles;

public static class ArticleMapper
{
    public static ArticleSummaryDto ToSummary(this Article a) => new(
        a.Id, a.Title, a.Slug, a.Lead, a.CoverImageUrl,
        new CategoryRefDto(a.Category.Id, a.Category.Name, a.Category.Slug),
        new AuthorDto(a.Author.Id, a.Author.FullName, a.Author.Position, a.Author.Bio, a.Author.AvatarUrl),
        a.Status, a.AccessLevel, a.CreatedAt, a.PublishedAt, a.ViewCount, a.SponsorPartnerId);

    public static ArticleDetailsDto ToDetails(this Article a) =>
        new(a.ToSummary(), a.Blocks.OrderBy(b => b.Position).Select(ToDto).ToList());

    public static BlockDto ToDto(this ContentBlock block) => (block switch
    {
        TextBlock t => new TextBlockDto(t.Body, t.Heading),
        ImageBlock i => new ImageBlockDto(i.Url, i.Caption),
        VideoBlock v => new VideoBlockDto(v.EmbedUrl, v.Caption),
        GalleryBlock g => (BlockDto)new GalleryBlockDto(g.Images.Select(img => new GalleryImageDto(img.Url, img.Caption)).ToList(), g.Title),
        _ => throw new NotSupportedException($"Невідомий тип блоку: {block.GetType().Name}")
    }) with { IsExclusive = block.IsExclusive };
}
