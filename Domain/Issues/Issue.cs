using CURSOVA.Domain.Common;

namespace CURSOVA.Domain.Issues;

public class Issue : Entity
{
    public const int TitleMaxLength = 150;
    public const int DescriptionMaxLength = 600;

    private readonly List<IssueComponent> _components = [];

    public string Title { get; private set; } = null!;
    public string Slug { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public string? CoverImageUrl { get; private set; }
    public bool IsPublished { get; private set; }
    public bool IsFeatured { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? PublishedAt { get; private set; }

    public IReadOnlyList<IssueComponent> Components => _components;

    public IssueSection Root => _components.OfType<IssueSection>().Single(c => c.Parent is null);

    private Issue() { }

    public Issue(string title, string description, string? coverImageUrl = null)
    {
        Update(title, description, coverImageUrl);
        CreatedAt = DateTime.UtcNow;
        _components.Add(new IssueSection("Зміст"));
    }

    public void Update(string title, string description, string? coverImageUrl)
    {
        Title = Guard.NotEmpty(title, "Назва випуску", TitleMaxLength);
        Slug = Common.Slug.From(Title);
        Description = Guard.NotEmpty(description, "Опис випуску", DescriptionMaxLength);
        CoverImageUrl = Guard.Optional(coverImageUrl, "Обкладинка", 500);
    }

    public IssueSection AddSection(IssueSection parent, string title)
    {
        EnsureOwned(parent);
        var section = new IssueSection(title);
        parent.Add(section);
        _components.Add(section);
        return section;
    }

    public IssueSection AddSection(int? parentId, string title) => AddSection(FindSection(parentId), title);

    public ArticleItem AddArticle(IssueSection parent, int articleId, string? editorNote = null)
    {
        EnsureOwned(parent);
        if (Root.GetArticleIds().Contains(articleId))
            throw new DomainException("Ця стаття вже є у випуску.");

        var item = new ArticleItem(articleId, editorNote);
        parent.Add(item);
        _components.Add(item);
        return item;
    }

    public ArticleItem AddArticle(int? parentId, int articleId, string? editorNote = null) =>
        AddArticle(FindSection(parentId), articleId, editorNote);

    public void Remove(int componentId)
    {
        var component = Find(componentId);
        if (component.Parent is null)
            throw new DomainException("Не можна видалити кореневий розділ.");

        foreach (var node in component.Flatten().ToList())
            _components.Remove(node);
        component.Parent.Remove(component);
    }

    public void Move(int componentId, int delta)
    {
        var component = Find(componentId);
        component.Parent?.Move(component, delta);
    }

    public void Publish()
    {
        if (Root.CountArticles() == 0)
            throw new DomainException("Не можна опублікувати порожній випуск.");

        IsPublished = true;
        PublishedAt ??= DateTime.UtcNow;
    }

    public void Unpublish()
    {
        IsPublished = false;
        IsFeatured = false;
    }

    public void Feature()
    {
        if (!IsPublished)
            throw new DomainException("«Журналом місяця» може бути лише опублікований випуск.");

        IsFeatured = true;
    }

    public void Unfeature() => IsFeatured = false;

    private IssueComponent Find(int componentId) =>
        _components.FirstOrDefault(c => c.Id == componentId)
        ?? throw new DomainException("Елемент випуску не знайдено.");

    private IssueSection FindSection(int? sectionId) =>
        sectionId is null ? Root
        : Find(sectionId.Value) as IssueSection ?? throw new DomainException("Статтю можна додати лише в розділ.");

    private void EnsureOwned(IssueComponent component)
    {
        if (!_components.Contains(component))
            throw new DomainException("Розділ належить іншому випуску.");
    }
}
