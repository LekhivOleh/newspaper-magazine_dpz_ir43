using CURSOVA.Domain.Common;

namespace CURSOVA.Domain.Articles.Blocks;

public abstract class ContentBlock : Entity
{
    public int ArticleId { get; private set; }
    public int Position { get; internal set; }

    public abstract BlockType Type { get; }
}

public enum BlockType
{
    Text,
    Image,
    Video,
    Gallery
}
