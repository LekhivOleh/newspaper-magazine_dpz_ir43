using CURSOVA.Domain.Common;

namespace CURSOVA.Domain.Articles.Blocks;

public class VideoBlock : ContentBlock
{
    public string EmbedUrl { get; private set; } = null!;
    public string? Caption { get; private set; }

    public override BlockType Type => BlockType.Video;

    private VideoBlock() { }

    public VideoBlock(string embedUrl, string? caption = null)
    {
        EmbedUrl = Guard.NotEmpty(embedUrl, "Посилання на відео", 500);
        Caption = Guard.Optional(caption, "Підпис", 300);
    }
}
