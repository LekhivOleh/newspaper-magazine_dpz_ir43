using CURSOVA.Application.Common;
using CURSOVA.Domain.Profiles;

namespace CURSOVA.Application.Abstractions.Persistence;

public interface ISavedArticleRepository
{
    Task<SavedArticle?> FindAsync(int userId, int articleId, CancellationToken ct = default);
    Task<PagedResult<SavedArticle>> ListAsync(int userId, int page, int pageSize, CancellationToken ct = default);
    void Add(SavedArticle saved);
    void Remove(SavedArticle saved);
}
