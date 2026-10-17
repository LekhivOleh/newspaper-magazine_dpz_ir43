using CURSOVA.Domain.Common;

namespace CURSOVA.Domain.Sharing;

public enum ShareableType
{
    Article,
    Quiz
}

public class ShareEvent : Entity
{
    public ShareableType ContentType { get; private set; }
    public int ContentId { get; private set; }
    public string Network { get; private set; } = null!;
    public DateTime CreatedAt { get; private set; }

    private ShareEvent() { }

    public ShareEvent(ShareableType contentType, int contentId, string network)
    {
        ContentType = contentType;
        ContentId = contentId;
        Network = Guard.NotEmpty(network, "Мережа", 30);
        CreatedAt = DateTime.UtcNow;
    }
}
