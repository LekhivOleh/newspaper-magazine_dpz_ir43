using CURSOVA.Domain.Articles.Blocks;

namespace CURSOVA.Domain.Articles.Building;

public class GalleryBuilder
{
    private readonly List<GalleryImage> _images = [];

    public GalleryBuilder Add(string url, string? caption = null)
    {
        _images.Add(new GalleryImage(url, caption));
        return this;
    }

    internal GalleryBlock Build(string? title) => new(_images, title);
}
