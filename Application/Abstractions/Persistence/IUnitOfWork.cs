namespace CURSOVA.Application.Abstractions.Persistence;

public interface IUnitOfWork
{
    ICategoryRepository Categories { get; }
    IAuthorRepository Authors { get; }
    IArticleRepository Articles { get; }
    IIssueRepository Issues { get; }

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
