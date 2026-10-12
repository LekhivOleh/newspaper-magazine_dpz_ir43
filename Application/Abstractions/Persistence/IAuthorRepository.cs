using CURSOVA.Domain.Authors;

namespace CURSOVA.Application.Abstractions.Persistence;

public interface IAuthorRepository : IRepository<Author>
{
    Task<bool> HasArticlesAsync(int authorId, CancellationToken ct = default);
}
