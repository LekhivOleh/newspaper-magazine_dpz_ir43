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

public class GalleryImage
{
    public string Url { get; private set; }
    public string? Caption { get; private set; }

    public GalleryImage(string url, string? caption = null)
    {
        Url = Guard.NotEmpty(url, "Посилання на зображення", 500);
        Caption = Guard.Optional(caption, "Підпис", 300);
    }
}
