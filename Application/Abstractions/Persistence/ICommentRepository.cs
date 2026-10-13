using CURSOVA.Application.Common;
using CURSOVA.Domain.Comments;

namespace CURSOVA.Application.Abstractions.Persistence;

public interface ICommentRepository : IRepository<Comment>
{
    /// <summary>Published comments plus the given user's own pending ones.</summary>
    Task<IReadOnlyList<Comment>> ListForArticleAsync(int articleId, int? viewerId, CancellationToken ct = default);
    Task<int> CountByUserSinceAsync(int userId, DateTime since, CancellationToken ct = default);
    Task<IReadOnlyList<string>> GetRecentBodiesByUserAsync(int userId, int take, CancellationToken ct = default);
    Task<PagedResult<Comment>> ListForModerationAsync(CommentStatus? status, int page, int pageSize, CancellationToken ct = default);
    Task<int> CountPendingAsync(CancellationToken ct = default);
}
