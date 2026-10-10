using CURSOVA.Domain.Common;

namespace CURSOVA.Domain.Articles.Blocks;

public class TextBlock : ContentBlock
{
    public string Body { get; private set; } = null!;
    public string? Heading { get; private set; }

    public override BlockType Type => BlockType.Text;

    private TextBlock() { }

    public TextBlock(string body, string? heading = null)
    {
        Body = Guard.NotEmpty(body, "Текст блоку");
        Heading = Guard.Optional(heading, "Підзаголовок", 200);
    }
}
