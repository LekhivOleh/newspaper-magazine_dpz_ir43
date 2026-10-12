using CURSOVA.Application.Abstractions.Persistence;

namespace CURSOVA.Infrastructure.Persistence;

public class UnitOfWork(
    MagazineDbContext db,
    ICategoryRepository categories,
    IAuthorRepository authors,
    IArticleRepository articles,
    IIssueRepository issues) : IUnitOfWork
{
    public ICategoryRepository Categories { get; } = categories;
    public IAuthorRepository Authors { get; } = authors;
    public IArticleRepository Articles { get; } = articles;
    public IIssueRepository Issues { get; } = issues;

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => db.SaveChangesAsync(ct);
}
