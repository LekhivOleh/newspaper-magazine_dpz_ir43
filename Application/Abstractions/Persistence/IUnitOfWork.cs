namespace CURSOVA.Application.Abstractions.Persistence;

public interface IUnitOfWork
{
    ICategoryRepository Categories { get; }
    IAuthorRepository Authors { get; }
    IArticleRepository Articles { get; }
    IIssueRepository Issues { get; }
    IInteractiveRepository Interactive { get; }
    ICommentRepository Comments { get; }
    IShareRepository Shares { get; }

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
