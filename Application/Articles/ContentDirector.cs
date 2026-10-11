using CURSOVA.Domain.Articles.Building;
using CURSOVA.Domain.Common;

namespace CURSOVA.Application.Articles;

public static class ContentDirector
{
    public static void Construct(IEnumerable<BlockDto> blocks, IContentBuilder builder)
    {
        foreach (var block in blocks)
        {
            switch (block)
            {
                case TextBlockDto t:
                    builder.AddText(t.Body, t.Heading);
                    break;
                case ImageBlockDto i:
                    builder.AddImage(i.Url, i.Caption);
                    break;
                case VideoBlockDto v:
                    builder.AddVideo(v.EmbedUrl, v.Caption);
                    break;
                case GalleryBlockDto g:
                    builder.AddGallery(gallery =>
                    {
                        foreach (var image in g.Images ?? [])
                            gallery.Add(image.Url, image.Caption);
                    }, g.Title);
                    break;
                default:
                    throw new DomainException("Невідомий тип блоку контенту.");
            }
        }
    }
}
