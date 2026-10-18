using CURSOVA.Domain.Common;

namespace CURSOVA.Domain.Profiles;

public class Collection : Entity
{
    public const int NameMaxLength = 100;
    public const int MaxItems = 50;

    private readonly List<CollectionItem> _items = [];

    public int UserId { get; private set; }
    public string OwnerName { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public bool IsPublic { get; private set; }
    public int? CopiedFromId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public IReadOnlyList<CollectionItem> Items => _items.OrderBy(i => i.Position).ToList();

    private Collection() { }

    public Collection(int userId, string ownerName, string name, string? description = null, bool isPublic = false)
    {
        UserId = userId;
        OwnerName = Guard.NotEmpty(ownerName, "Власник", 100);
        CreatedAt = DateTime.UtcNow;
        Update(name, description, isPublic);
    }

    public void Update(string name, string? description, bool isPublic)
    {
        Name = Guard.NotEmpty(name, "Назва підбірки", NameMaxLength);
        Description = Guard.Optional(description, "Опис", 500);
        IsPublic = isPublic;
    }

    public bool IsOwnedBy(int? userId) => userId == UserId;

    public bool CanBeViewedBy(int? userId) => IsPublic || IsOwnedBy(userId);

    public bool Contains(int articleId) => _items.Any(i => i.ArticleId == articleId);

    public void AddArticle(int articleId, string? note = null)
    {
        if (Contains(articleId))
            throw new DomainException("Ця стаття вже є в підбірці.");
        if (_items.Count >= MaxItems)
            throw new DomainException($"У підбірці може бути не більше {MaxItems} статей.");

        var position = _items.Count == 0 ? 0 : _items.Max(i => i.Position) + 1;
        _items.Add(new CollectionItem(articleId, position, note));
    }

    public void RemoveArticle(int articleId)
    {
        var item = _items.FirstOrDefault(i => i.ArticleId == articleId)
                   ?? throw new DomainException("Цієї статті немає в підбірці.");
        _items.Remove(item);
    }

    public void Move(int articleId, int delta)
    {
        var ordered = Items.ToList();
        var index = ordered.FindIndex(i => i.ArticleId == articleId);
        var target = index + delta;
        if (index < 0 || target < 0 || target >= ordered.Count)
            return;

        (ordered[index].Position, ordered[target].Position) = (ordered[target].Position, ordered[index].Position);
    }

    /// <summary>
    /// Prototype: the collection copies itself — items, order and notes included —
    /// so callers don't need to know how a collection is put together.
    /// </summary>
    public Collection CloneFor(int userId, string ownerName)
    {
        if (IsOwnedBy(userId))
            throw new DomainException("Це ваша власна підбірка.");

        var copy = new Collection(userId, ownerName, Name, Description, isPublic: false) { CopiedFromId = Id };
        foreach (var item in Items)
            copy._items.Add(item.Clone());
        return copy;
    }
}

public class CollectionItem : Entity
{
    public int CollectionId { get; private set; }
    public int ArticleId { get; private set; }
    public string? Note { get; private set; }
    public int Position { get; internal set; }
    public DateTime AddedAt { get; private set; }

    private CollectionItem() { }

    internal CollectionItem(int articleId, int position, string? note)
    {
        ArticleId = articleId;
        Position = position;
        Note = Guard.Optional(note, "Нотатка", 300);
        AddedAt = DateTime.UtcNow;
    }

    internal CollectionItem Clone() => new(ArticleId, Position, Note);
}
