using CURSOVA.Domain.Common;

namespace CURSOVA.Domain.Articles.Blocks;

public class GalleryBlock : ContentBlock
{
    private readonly List<GalleryImage> _images = [];

    public string? Title { get; private set; }
    public IReadOnlyList<GalleryImage> Images => _images;

    public override BlockType Type => BlockType.Gallery;

    private GalleryBlock() { }

    public GalleryBlock(IEnumerable<GalleryImage> images, string? title = null)
    {
        _images.AddRange(images);
        if (_images.Count == 0)
            throw new DomainException("Галерея має містити хоча б одне зображення.");

        Title = Guard.Optional(title, "Назва галереї", 200);
    }
}

public record GalleryImage(string Url, string? Caption);
