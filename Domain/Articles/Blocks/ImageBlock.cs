using CURSOVA.Domain.Common;

namespace CURSOVA.Domain.Articles.Blocks;

public class ImageBlock : ContentBlock
{
    public string Url { get; private set; } = null!;
    public string? Caption { get; private set; }

    public override BlockType Type => BlockType.Image;

    private ImageBlock() { }

    public ImageBlock(string url, string? caption = null)
    {
        Url = Guard.NotEmpty(url, "Посилання на зображення", 500);
        Caption = Guard.Optional(caption, "Підпис", 300);
    }
}
