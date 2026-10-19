using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using CURSOVA.Domain.Articles;

namespace CURSOVA.Application.Articles;

public record CategoryRefDto(int Id, string Name, string Slug);

public record AuthorDto(int Id, string FullName, string? Position, string? Bio, string? AvatarUrl);

public record ArticleSummaryDto(
    int Id,
    string Title,
    string Slug,
    string Lead,
    string? CoverImageUrl,
    CategoryRefDto Category,
    AuthorDto Author,
    ArticleStatus Status,
    AccessLevel AccessLevel,
    DateTime CreatedAt,
    DateTime? PublishedAt,
    int ViewCount,
    int? SponsorPartnerId = null);

public record ArticleDetailsDto(ArticleSummaryDto Summary, IReadOnlyList<BlockDto> Blocks, ContentAccessDto? Access = null);

/// <param name="IsFull">False when the reader sees only a teaser of a Premium article.</param>
/// <param name="HiddenBlocks">How many blocks were held back (teaser cut or locked exclusive media).</param>
public record ContentAccessDto(bool IsFull, int HiddenBlocks, bool HasPremium)
{
    public static readonly ContentAccessDto Unrestricted = new(true, 0, false);
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(TextBlockDto), "text")]
[JsonDerivedType(typeof(ImageBlockDto), "image")]
[JsonDerivedType(typeof(VideoBlockDto), "video")]
[JsonDerivedType(typeof(GalleryBlockDto), "gallery")]
[JsonDerivedType(typeof(LockedBlockDto), "locked")]
public abstract record BlockDto
{
    public bool IsExclusive { get; init; }
}

/// <summary>Placeholder the reader sees instead of exclusive media they can't access.</summary>
public record LockedBlockDto(string LockedType) : BlockDto;

public record TextBlockDto(string Body, string? Heading) : BlockDto;

public record ImageBlockDto(string Url, string? Caption) : BlockDto;

public record VideoBlockDto(string EmbedUrl, string? Caption) : BlockDto;

public record GalleryBlockDto(IReadOnlyList<GalleryImageDto> Images, string? Title) : BlockDto;

public record GalleryImageDto(string Url, string? Caption);

public record ArticleFilter
{
    public int? CategoryId { get; init; }
    public string? Category { get; init; }
    public int? AuthorId { get; init; }
    public AccessLevel? Access { get; init; }
    public ArticleStatus? Status { get; init; }
    public string? Search { get; init; }
    [Range(1, int.MaxValue)] public int Page { get; init; } = 1;
    [Range(1, 50)] public int PageSize { get; init; } = 10;
}

public record SaveContentRequest([Required] IReadOnlyList<BlockDto> Blocks);

public record SaveArticleRequest(
    [Required, MaxLength(Article.TitleMaxLength)] string Title,
    [Required, MaxLength(Article.LeadMaxLength)] string Lead,
    [Range(1, int.MaxValue)] int CategoryId,
    [Range(1, int.MaxValue)] int AuthorId,
    [MaxLength(500), Url] string? CoverImageUrl,
    AccessLevel AccessLevel = AccessLevel.Free,
    int? SponsorPartnerId = null);
