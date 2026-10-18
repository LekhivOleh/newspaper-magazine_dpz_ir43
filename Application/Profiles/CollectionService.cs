using System.ComponentModel.DataAnnotations;
using CURSOVA.Application.Abstractions.Identity;
using CURSOVA.Application.Abstractions.Persistence;
using CURSOVA.Application.Articles;
using CURSOVA.Application.Common;
using CURSOVA.Domain.Articles;
using CURSOVA.Domain.Common;
using CURSOVA.Domain.Profiles;

namespace CURSOVA.Application.Profiles;

public record CollectionSummaryDto(
    int Id,
    string Name,
    string? Description,
    bool IsPublic,
    string OwnerName,
    bool IsMine,
    int ItemCount,
    string? CoverImageUrl,
    int? CopiedFromId,
    DateTime CreatedAt);

public record CollectionItemDto(ArticleSummaryDto Article, string? Note, DateTime AddedAt);

public record CollectionDto(CollectionSummaryDto Summary, IReadOnlyList<CollectionItemDto> Items);

/// <summary>Collection option for the "add to collection" picker on an article page.</summary>
public record CollectionChoiceDto(int Id, string Name, bool ContainsArticle);

public record SaveCollectionRequest(
    [Required, MaxLength(Collection.NameMaxLength)] string Name,
    [MaxLength(500)] string? Description,
    bool IsPublic);

public record AddToCollectionRequest(int ArticleId, [MaxLength(300)] string? Note);

public interface ICollectionService
{
    Task<IReadOnlyList<CollectionSummaryDto>> ListMineAsync(CancellationToken ct = default);
    Task<IReadOnlyList<CollectionSummaryDto>> ListPublicAsync(CancellationToken ct = default);
    Task<IReadOnlyList<CollectionChoiceDto>> ChoicesForArticleAsync(int articleId, CancellationToken ct = default);
    Task<CollectionDto> GetAsync(int id, CancellationToken ct = default);
    Task<CollectionSummaryDto> CreateAsync(SaveCollectionRequest request, CancellationToken ct = default);
    Task UpdateAsync(int id, SaveCollectionRequest request, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
    Task AddArticleAsync(int id, int articleId, string? note, CancellationToken ct = default);
    Task RemoveArticleAsync(int id, int articleId, CancellationToken ct = default);
    Task MoveArticleAsync(int id, int articleId, int delta, CancellationToken ct = default);
    Task<int> CopyAsync(int id, CancellationToken ct = default);
}

public class CollectionService(IUnitOfWork uow, ICurrentUser currentUser) : ICollectionService
{
    private const int MaxCollectionsPerUser = 30;

    public async Task<IReadOnlyList<CollectionSummaryDto>> ListMineAsync(CancellationToken ct = default) =>
        await ToSummariesAsync(await uow.Collections.ListByUserAsync(RequireUser(), ct), ct);

    public async Task<IReadOnlyList<CollectionSummaryDto>> ListPublicAsync(CancellationToken ct = default) =>
        await ToSummariesAsync(await uow.Collections.ListPublicAsync(24, ct), ct);

    public async Task<IReadOnlyList<CollectionChoiceDto>> ChoicesForArticleAsync(int articleId, CancellationToken ct = default) =>
        currentUser.Id is not { } userId
            ? []
            : (await uow.Collections.ListByUserAsync(userId, ct))
                .Select(c => new CollectionChoiceDto(c.Id, c.Name, c.Contains(articleId)))
                .ToList();

    public async Task<CollectionDto> GetAsync(int id, CancellationToken ct = default)
    {
        var collection = await uow.Collections.GetWithItemsAsync(id, ct);
        if (collection is null || !collection.CanBeViewedBy(currentUser.Id))
            throw NotFoundException.For<Collection>(id);

        var articles = await PublishedArticlesAsync(collection.Items.Select(i => i.ArticleId), ct);
        var items = collection.Items
            .Where(i => articles.ContainsKey(i.ArticleId))
            .Select(i => new CollectionItemDto(articles[i.ArticleId], i.Note, i.AddedAt))
            .ToList();

        return new CollectionDto(ToSummary(collection, articles), items);
    }

    public async Task<CollectionSummaryDto> CreateAsync(SaveCollectionRequest request, CancellationToken ct = default)
    {
        var userId = RequireUser();
        if ((await uow.Collections.ListByUserAsync(userId, ct)).Count >= MaxCollectionsPerUser)
            throw new DomainException($"Можна мати не більше {MaxCollectionsPerUser} підбірок.");

        var collection = new Collection(userId, OwnerName, request.Name, request.Description, request.IsPublic);
        uow.Collections.Add(collection);
        await uow.SaveChangesAsync(ct);
        return ToSummary(collection, new Dictionary<int, ArticleSummaryDto>());
    }

    public Task UpdateAsync(int id, SaveCollectionRequest request, CancellationToken ct = default) =>
        ChangeAsync(id, c => c.Update(request.Name, request.Description, request.IsPublic), ct);

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        uow.Collections.Remove(await LoadOwnedAsync(id, ct));
        await uow.SaveChangesAsync(ct);
    }

    public async Task AddArticleAsync(int id, int articleId, string? note, CancellationToken ct = default)
    {
        var article = await uow.Articles.GetByIdAsync(articleId, ct) ?? throw NotFoundException.For<Article>(articleId);
        if (!article.IsPublished)
            throw new DomainException("До підбірки можна додати лише опубліковану статтю.");

        await ChangeAsync(id, c => c.AddArticle(articleId, note), ct);
    }

    public Task RemoveArticleAsync(int id, int articleId, CancellationToken ct = default) =>
        ChangeAsync(id, c => c.RemoveArticle(articleId), ct);

    public Task MoveArticleAsync(int id, int articleId, int delta, CancellationToken ct = default) =>
        ChangeAsync(id, c => c.Move(articleId, Math.Sign(delta)), ct);

    public async Task<int> CopyAsync(int id, CancellationToken ct = default)
    {
        var userId = RequireUser();
        var original = await uow.Collections.GetWithItemsAsync(id, ct);
        if (original is null || !original.CanBeViewedBy(userId))
            throw NotFoundException.For<Collection>(id);

        var copy = original.CloneFor(userId, OwnerName);
        uow.Collections.Add(copy);
        await uow.SaveChangesAsync(ct);
        return copy.Id;
    }

    private async Task ChangeAsync(int id, Action<Collection> change, CancellationToken ct)
    {
        change(await LoadOwnedAsync(id, ct));
        await uow.SaveChangesAsync(ct);
    }

    // Someone else's collection is reported as missing rather than forbidden, so ids can't be probed.
    private async Task<Collection> LoadOwnedAsync(int id, CancellationToken ct)
    {
        var collection = await uow.Collections.GetWithItemsAsync(id, ct);
        if (collection is null || !collection.IsOwnedBy(RequireUser()))
            throw NotFoundException.For<Collection>(id);
        return collection;
    }

    private async Task<IReadOnlyList<CollectionSummaryDto>> ToSummariesAsync(IReadOnlyList<Collection> collections, CancellationToken ct)
    {
        var articles = await PublishedArticlesAsync(collections.SelectMany(c => c.Items.Select(i => i.ArticleId)), ct);
        return collections.Select(c => ToSummary(c, articles)).ToList();
    }

    private async Task<Dictionary<int, ArticleSummaryDto>> PublishedArticlesAsync(IEnumerable<int> ids, CancellationToken ct) =>
        (await uow.Articles.GetByIdsAsync(ids, ct))
        .Where(a => a.IsPublished)
        .ToDictionary(a => a.Id, a => a.ToSummary());

    private CollectionSummaryDto ToSummary(Collection c, IReadOnlyDictionary<int, ArticleSummaryDto> articles)
    {
        var visible = c.Items.Where(i => articles.ContainsKey(i.ArticleId)).ToList();
        return new CollectionSummaryDto(
            c.Id, c.Name, c.Description, c.IsPublic, c.OwnerName, c.IsOwnedBy(currentUser.Id), visible.Count,
            visible.Select(i => articles[i.ArticleId].CoverImageUrl).FirstOrDefault(url => url is not null),
            c.CopiedFromId, c.CreatedAt);
    }

    private int RequireUser() => currentUser.Id ?? throw new DomainException("Увійдіть, щоб працювати з підбірками.");

    private string OwnerName => currentUser.DisplayName ?? "Читач";
}
