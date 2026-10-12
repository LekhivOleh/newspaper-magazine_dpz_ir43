using CURSOVA.Application.Articles;
using CURSOVA.Domain.Issues;

namespace CURSOVA.Application.Issues;

public static class IssueMapper
{
    public static IssueSummaryDto ToSummary(this Issue issue) => new(
        issue.Id, issue.Title, issue.Slug, issue.Description, issue.CoverImageUrl,
        issue.IsPublished, issue.IsFeatured, issue.CreatedAt, issue.PublishedAt,
        issue.Root.CountArticles());

    public static IssueDetailsDto ToDetails(this Issue issue, IReadOnlyDictionary<int, ArticleSummaryDto> articles) =>
        new(issue.ToSummary(), (SectionNodeDto)ToNode(issue.Root, articles)!);

    // Articles missing from the dictionary (deleted or hidden from the reader) are skipped.
    private static IssueNodeDto? ToNode(IssueComponent component, IReadOnlyDictionary<int, ArticleSummaryDto> articles) =>
        component switch
        {
            IssueSection section => new SectionNodeDto(
                section.Id,
                section.Title,
                section.GetArticleIds().Count(articles.ContainsKey),
                section.Children.Select(c => ToNode(c, articles)).OfType<IssueNodeDto>().ToList()),
            ArticleItem item when articles.TryGetValue(item.ArticleId, out var article) =>
                new ArticleNodeDto(item.Id, item.EditorNote, article),
            _ => null
        };
}
