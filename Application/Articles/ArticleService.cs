using CURSOVA.Application.Abstractions.Persistence;
using CURSOVA.Application.Common;
using CURSOVA.Application.Notifications;
using CURSOVA.Domain.Articles;
using CURSOVA.Domain.Articles.Building;
using CURSOVA.Domain.Authors;
using CURSOVA.Domain.Categories;

namespace CURSOVA.Application.Articles;

public class ArticleService(IUnitOfWork uow, IMagazineEventPublisher events) : IArticleService
{
    public Task<PagedResult<ArticleSummaryDto>> SearchAsync(ArticleFilter filter, CancellationToken ct = default) =>
        QueryAsync(filter, filter.Status ?? ArticleStatus.Published, ct);

    public Task<PagedResult<ArticleSummaryDto>> SearchAllAsync(ArticleFilter filter, CancellationToken ct = default) =>
        QueryAsync(filter, filter.Status, ct);

    private async Task<PagedResult<ArticleSummaryDto>> QueryAsync(ArticleFilter filter, ArticleStatus? status, CancellationToken ct)
    {
        var categoryId = filter.CategoryId;
        if (categoryId is null && !string.IsNullOrWhiteSpace(filter.Category))
        {
            var category = await uow.Categories.GetBySlugAsync(filter.Category, ct)
                           ?? throw NotFoundException.For<Category>(filter.Category);
            categoryId = category.Id;
        }

        var page = await uow.Articles.SearchAsync(new ArticleQuery
        {
            CategoryId = categoryId,
            AuthorId = filter.AuthorId,
            AccessLevel = filter.Access,
            Status = status,
            Search = filter.Search,
            Page = filter.Page,
            PageSize = filter.PageSize
        }, ct);

        return page.Map(a => a.ToSummary());
    }

    public async Task<ArticleDetailsDto> GetAsync(int id, CancellationToken ct = default) =>
        (await LoadAsync(id, ct)).ToDetails();

    public async Task<ArticleDetailsDto> GetBySlugAsync(string slug, CancellationToken ct = default)
    {
        var article = await uow.Articles.GetBySlugWithContentAsync(slug, ct)
                      ?? throw NotFoundException.For<Article>(slug);
        return article.ToDetails();
    }

    public async Task<ArticleDetailsDto> CreateAsync(SaveArticleRequest request, CancellationToken ct = default)
    {
        await EnsureReferencesExistAsync(request, ct);

        var article = ArticleBuilder.Create(request.Title)
            .WithLead(request.Lead)
            .WithCover(request.CoverImageUrl)
            .InCategory(request.CategoryId)
            .By(request.AuthorId)
            .WithAccess(request.AccessLevel)
            .Build();
        article.SetSponsor(request.SponsorPartnerId);
        await EnsureUniqueSlugAsync(article, ct);

        uow.Articles.Add(article);
        await uow.SaveChangesAsync(ct);
        return await GetAsync(article.Id, ct);
    }

    public async Task<ArticleDetailsDto> UpdateAsync(int id, SaveArticleRequest request, CancellationToken ct = default)
    {
        var article = await LoadAsync(id, ct);
        await EnsureReferencesExistAsync(request, ct);

        article.UpdateDetails(request.Title, request.Lead, request.CoverImageUrl);
        article.MoveToCategory(request.CategoryId);
        article.AssignAuthor(request.AuthorId);
        article.SetAccessLevel(request.AccessLevel);
        article.SetSponsor(request.SponsorPartnerId);
        await EnsureUniqueSlugAsync(article, ct);

        await uow.SaveChangesAsync(ct);
        return await GetAsync(id, ct);
    }

    public Task<ArticleDetailsDto> ReplaceContentAsync(int id, SaveContentRequest request, CancellationToken ct = default)
    {
        var content = new ContentBuilder();
        ContentDirector.Construct(request.Blocks, content);
        return ChangeAsync(id, a => a.ReplaceContent(content.Build()), ct);
    }

    public async Task<ArticleDetailsDto> PublishAsync(int id, CancellationToken ct = default)
    {
        var article = await LoadAsync(id, ct);
        var firstRelease = article.PublishedAt is null;
        article.Publish();
        await uow.SaveChangesAsync(ct);

        if (firstRelease && (article.IsPremium || article.Blocks.Any(b => b.IsExclusive)))
            await events.PublishAsync(new ExclusiveContentPublished(
                article.Id, article.Title, article.Slug, article.Lead, article.CoverImageUrl, article.IsPremium), ct);

        return article.ToDetails();
    }

    public Task<ArticleDetailsDto> ArchiveAsync(int id, CancellationToken ct = default) =>
        ChangeAsync(id, a => a.Archive(), ct);

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var article = await LoadAsync(id, ct);
        uow.Articles.Remove(article);
        await uow.SaveChangesAsync(ct);
    }

    public async Task RegisterViewAsync(int id, CancellationToken ct = default)
    {
        var article = await uow.Articles.GetByIdAsync(id, ct) ?? throw NotFoundException.For<Article>(id);
        article.RegisterView();
        await uow.SaveChangesAsync(ct);
    }

    private async Task<ArticleDetailsDto> ChangeAsync(int id, Action<Article> change, CancellationToken ct)
    {
        var article = await LoadAsync(id, ct);
        change(article);
        await uow.SaveChangesAsync(ct);
        return article.ToDetails();
    }

    private async Task<Article> LoadAsync(int id, CancellationToken ct) =>
        await uow.Articles.GetWithContentAsync(id, ct) ?? throw NotFoundException.For<Article>(id);

    private async Task EnsureReferencesExistAsync(SaveArticleRequest request, CancellationToken ct)
    {
        _ = await uow.Categories.GetByIdAsync(request.CategoryId, ct)
            ?? throw NotFoundException.For<Category>(request.CategoryId);
        _ = await uow.Authors.GetByIdAsync(request.AuthorId, ct)
            ?? throw NotFoundException.For<Author>(request.AuthorId);
    }

    private async Task EnsureUniqueSlugAsync(Article article, CancellationToken ct)
    {
        var excludeId = article.Id == 0 ? (int?)null : article.Id;
        if (await uow.Articles.SlugExistsAsync(article.Slug, excludeId, ct))
            throw new ConflictException($"Стаття із заголовком «{article.Title}» уже існує.");
    }
}
