using CURSOVA.Application.Abstractions.Persistence;
using CURSOVA.Application.Articles;
using CURSOVA.Application.Common;
using CURSOVA.Domain.Articles;
using CURSOVA.Domain.Issues;

namespace CURSOVA.Application.Issues;

public interface IIssueService
{
    Task<IReadOnlyList<IssueSummaryDto>> ListAsync(bool publishedOnly, CancellationToken ct = default);
    Task<IssueSummaryDto?> GetFeaturedAsync(CancellationToken ct = default);
    Task<IssueDetailsDto> GetAsync(int id, CancellationToken ct = default);
    Task<IssueDetailsDto> GetPublishedBySlugAsync(string slug, CancellationToken ct = default);
    Task<IssueDetailsDto> CreateAsync(SaveIssueRequest request, CancellationToken ct = default);
    Task<IssueDetailsDto> UpdateAsync(int id, SaveIssueRequest request, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
    Task AddSectionAsync(int issueId, int? parentId, string title, CancellationToken ct = default);
    Task AddArticleAsync(int issueId, int? parentId, int articleId, string? editorNote, CancellationToken ct = default);
    Task RemoveComponentAsync(int issueId, int componentId, CancellationToken ct = default);
    Task MoveComponentAsync(int issueId, int componentId, int delta, CancellationToken ct = default);
    Task PublishAsync(int id, CancellationToken ct = default);
    Task UnpublishAsync(int id, CancellationToken ct = default);
    Task FeatureAsync(int id, CancellationToken ct = default);
    Task UnfeatureAsync(int id, CancellationToken ct = default);
}

public class IssueService(IUnitOfWork uow) : IIssueService
{
    public async Task<IReadOnlyList<IssueSummaryDto>> ListAsync(bool publishedOnly, CancellationToken ct = default) =>
        (await uow.Issues.ListWithTreeAsync(publishedOnly, ct)).Select(i => i.ToSummary()).ToList();

    public async Task<IssueSummaryDto?> GetFeaturedAsync(CancellationToken ct = default) =>
        (await uow.Issues.GetFeaturedAsync(ct)).FirstOrDefault(i => i.IsPublished)?.ToSummary();

    public async Task<IssueDetailsDto> GetAsync(int id, CancellationToken ct = default) =>
        await ToDetailsAsync(await LoadAsync(id, ct), publishedArticlesOnly: false, ct);

    public async Task<IssueDetailsDto> GetPublishedBySlugAsync(string slug, CancellationToken ct = default)
    {
        var issue = await uow.Issues.GetBySlugWithTreeAsync(slug, ct);
        if (issue is not { IsPublished: true })
            throw NotFoundException.For<Issue>(slug);

        return await ToDetailsAsync(issue, publishedArticlesOnly: true, ct);
    }

    public async Task<IssueDetailsDto> CreateAsync(SaveIssueRequest request, CancellationToken ct = default)
    {
        var issue = new Issue(request.Title, request.Description, request.CoverImageUrl);
        await EnsureUniqueSlugAsync(issue, ct);

        uow.Issues.Add(issue);
        await uow.SaveChangesAsync(ct);
        return await GetAsync(issue.Id, ct);
    }

    public Task<IssueDetailsDto> UpdateAsync(int id, SaveIssueRequest request, CancellationToken ct = default) =>
        ChangeAsync(id, async issue =>
        {
            issue.Update(request.Title, request.Description, request.CoverImageUrl);
            await EnsureUniqueSlugAsync(issue, ct);
        }, ct);

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        uow.Issues.Remove(await LoadAsync(id, ct));
        await uow.SaveChangesAsync(ct);
    }

    public Task AddSectionAsync(int issueId, int? parentId, string title, CancellationToken ct = default) =>
        ChangeAsync(issueId, issue => issue.AddSection(parentId, title), ct);

    public async Task AddArticleAsync(int issueId, int? parentId, int articleId, string? editorNote, CancellationToken ct = default)
    {
        _ = await uow.Articles.GetByIdAsync(articleId, ct) ?? throw NotFoundException.For<Article>(articleId);
        await ChangeAsync(issueId, issue => issue.AddArticle(parentId, articleId, editorNote), ct);
    }

    public Task RemoveComponentAsync(int issueId, int componentId, CancellationToken ct = default) =>
        ChangeAsync(issueId, issue => issue.Remove(componentId), ct);

    public Task MoveComponentAsync(int issueId, int componentId, int delta, CancellationToken ct = default) =>
        ChangeAsync(issueId, issue => issue.Move(componentId, delta), ct);

    public Task PublishAsync(int id, CancellationToken ct = default) => ChangeAsync(id, issue => issue.Publish(), ct);

    public Task UnpublishAsync(int id, CancellationToken ct = default) => ChangeAsync(id, issue => issue.Unpublish(), ct);

    public async Task FeatureAsync(int id, CancellationToken ct = default)
    {
        var issue = await LoadAsync(id, ct);
        foreach (var other in await uow.Issues.GetFeaturedAsync(ct))
            other.Unfeature();

        issue.Feature();
        await uow.SaveChangesAsync(ct);
    }

    public Task UnfeatureAsync(int id, CancellationToken ct = default) => ChangeAsync(id, issue => issue.Unfeature(), ct);

    private Task<IssueDetailsDto> ChangeAsync(int id, Action<Issue> change, CancellationToken ct) =>
        ChangeAsync(id, issue =>
        {
            change(issue);
            return Task.CompletedTask;
        }, ct);

    private async Task<IssueDetailsDto> ChangeAsync(int id, Func<Issue, Task> change, CancellationToken ct)
    {
        var issue = await LoadAsync(id, ct);
        await change(issue);
        await uow.SaveChangesAsync(ct);
        return await ToDetailsAsync(issue, publishedArticlesOnly: false, ct);
    }

    private async Task<Issue> LoadAsync(int id, CancellationToken ct) =>
        await uow.Issues.GetWithTreeAsync(id, ct) ?? throw NotFoundException.For<Issue>(id);

    private async Task<IssueDetailsDto> ToDetailsAsync(Issue issue, bool publishedArticlesOnly, CancellationToken ct)
    {
        var articles = await uow.Articles.GetByIdsAsync(issue.Root.GetArticleIds(), ct);
        var visible = articles
            .Where(a => !publishedArticlesOnly || a.IsPublished)
            .ToDictionary(a => a.Id, a => a.ToSummary());

        return issue.ToDetails(visible);
    }

    private async Task EnsureUniqueSlugAsync(Issue issue, CancellationToken ct)
    {
        var excludeId = issue.Id == 0 ? (int?)null : issue.Id;
        if (await uow.Issues.SlugExistsAsync(issue.Slug, excludeId, ct))
            throw new ConflictException($"Випуск із назвою «{issue.Title}» уже існує.");
    }
}
