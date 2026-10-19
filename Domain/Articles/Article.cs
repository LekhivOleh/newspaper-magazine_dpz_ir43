using CURSOVA.Domain.Articles.Blocks;
using CURSOVA.Domain.Authors;
using CURSOVA.Domain.Categories;
using CURSOVA.Domain.Common;

namespace CURSOVA.Domain.Articles;

public class Article : Entity
{
    public const int TitleMaxLength = 200;
    public const int LeadMaxLength = 600;

    private readonly List<ContentBlock> _blocks = [];

    public string Title { get; private set; } = null!;
    public string Slug { get; private set; } = null!;
    public string Lead { get; private set; } = null!;
    public string? CoverImageUrl { get; private set; }

    // SQLite LIKE is case-insensitive only for ASCII, so Cyrillic search goes through a pre-lowered copy.
    public string SearchText { get; private set; } = null!;

    public int CategoryId { get; private set; }
    public Category Category { get; private set; } = null!;
    public int AuthorId { get; private set; }
    public Author Author { get; private set; } = null!;

    public ArticleStatus Status { get; private set; } = ArticleStatus.Draft;
    public AccessLevel AccessLevel { get; private set; } = AccessLevel.Free;
    public DateTime CreatedAt { get; private set; }
    public DateTime? PublishedAt { get; private set; }
    public int ViewCount { get; private set; }

    public IReadOnlyList<ContentBlock> Blocks => _blocks;

    public bool IsPublished => Status == ArticleStatus.Published;
    public bool IsPremium => AccessLevel == AccessLevel.Premium;

    private Article() { }

    public Article(string title, string lead, int categoryId, int authorId, string? coverImageUrl = null)
    {
        UpdateDetails(title, lead, coverImageUrl);
        MoveToCategory(categoryId);
        AssignAuthor(authorId);
        CreatedAt = DateTime.UtcNow;
    }

    public void AssignAuthor(int authorId)
    {
        if (authorId <= 0)
            throw new DomainException("Стаття має мати автора.");

        AuthorId = authorId;
    }

    public void UpdateDetails(string title, string lead, string? coverImageUrl)
    {
        Title = Guard.NotEmpty(title, "Заголовок", TitleMaxLength);
        Slug = Common.Slug.From(Title);
        Lead = Guard.NotEmpty(lead, "Лід", LeadMaxLength);
        CoverImageUrl = Guard.Optional(coverImageUrl, "Обкладинка", 500);
        SearchText = $"{Title} {Lead}".ToLowerInvariant();
    }

    public void MoveToCategory(int categoryId)
    {
        if (categoryId <= 0)
            throw new DomainException("Стаття має належати до рубрики.");

        CategoryId = categoryId;
    }

    public void SetAccessLevel(AccessLevel level) => AccessLevel = level;

    /// <summary>Brand integration: the article is published as partner (sponsored) content.</summary>
    public int? SponsorPartnerId { get; private set; }

    public void SetSponsor(int? partnerId) => SponsorPartnerId = partnerId is > 0 ? partnerId : null;

    public void ReplaceContent(IEnumerable<ContentBlock> blocks)
    {
        var newBlocks = blocks.ToList();
        if (newBlocks.Count == 0 && IsPublished)
            throw new DomainException("Опублікована стаття не може залишитися без контенту.");

        _blocks.Clear();
        foreach (var block in newBlocks)
        {
            block.Position = _blocks.Count;
            _blocks.Add(block);
        }
    }

    public void Publish()
    {
        if (Status == ArticleStatus.Published)
            return;
        if (_blocks.Count == 0)
            throw new DomainException("Не можна опублікувати статтю без контенту.");

        Status = ArticleStatus.Published;
        PublishedAt ??= DateTime.UtcNow;
    }

    public void Archive() => Status = ArticleStatus.Archived;

    public void RegisterView() => ViewCount++;
}
