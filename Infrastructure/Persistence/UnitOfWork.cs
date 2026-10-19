using CURSOVA.Application.Abstractions.Persistence;

namespace CURSOVA.Infrastructure.Persistence;

public class UnitOfWork(
    MagazineDbContext db,
    ICategoryRepository categories,
    IAuthorRepository authors,
    IArticleRepository articles,
    IIssueRepository issues,
    IInteractiveRepository interactive,
    ICommentRepository comments,
    IShareRepository shares,
    ISavedArticleRepository savedArticles,
    ICollectionRepository collections,
    ISubscriptionRepository subscriptions,
    IAdvertisingRepository advertising) : IUnitOfWork
{
    public ICategoryRepository Categories { get; } = categories;
    public IAuthorRepository Authors { get; } = authors;
    public IArticleRepository Articles { get; } = articles;
    public IIssueRepository Issues { get; } = issues;
    public IInteractiveRepository Interactive { get; } = interactive;
    public ICommentRepository Comments { get; } = comments;
    public IShareRepository Shares { get; } = shares;
    public ISavedArticleRepository SavedArticles { get; } = savedArticles;
    public ICollectionRepository Collections { get; } = collections;
    public ISubscriptionRepository Subscriptions { get; } = subscriptions;
    public IAdvertisingRepository Advertising { get; } = advertising;

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => db.SaveChangesAsync(ct);
}
