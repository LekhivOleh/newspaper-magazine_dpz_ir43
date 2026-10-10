using CURSOVA.Application.Abstractions.Persistence;

namespace CURSOVA.Infrastructure.Persistence;

public class UnitOfWork(
    MagazineDbContext db,
    ICategoryRepository categories,
    IAuthorRepository authors,
    IArticleRepository articles) : IUnitOfWork
{
    public ICategoryRepository Categories { get; } = categories;
    public IAuthorRepository Authors { get; } = authors;
    public IArticleRepository Articles { get; } = articles;

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => db.SaveChangesAsync(ct);
}
