using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using CURSOVA.Application.Articles;
using CURSOVA.Domain.Issues;

namespace CURSOVA.Application.Issues;

public record IssueSummaryDto(
    int Id,
    string Title,
    string Slug,
    string Description,
    string? CoverImageUrl,
    bool IsPublished,
    bool IsFeatured,
    DateTime CreatedAt,
    DateTime? PublishedAt,
    int ArticleCount);

public record IssueDetailsDto(IssueSummaryDto Summary, SectionNodeDto Root);

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(SectionNodeDto), "section")]
[JsonDerivedType(typeof(ArticleNodeDto), "article")]
public abstract record IssueNodeDto(int Id);

public record SectionNodeDto(int Id, string Title, int ArticleCount, IReadOnlyList<IssueNodeDto> Children) : IssueNodeDto(Id)
{
    public IEnumerable<IssueNodeDto> Descendants() =>
        Children.SelectMany(c => c is SectionNodeDto s ? s.Descendants().Prepend(c) : [c]);
}

public record ArticleNodeDto(int Id, string? EditorNote, ArticleSummaryDto Article) : IssueNodeDto(Id);

public record SaveIssueRequest(
    [Required, MaxLength(Issue.TitleMaxLength)] string Title,
    [Required, MaxLength(Issue.DescriptionMaxLength)] string Description,
    [MaxLength(500), Url] string? CoverImageUrl);
