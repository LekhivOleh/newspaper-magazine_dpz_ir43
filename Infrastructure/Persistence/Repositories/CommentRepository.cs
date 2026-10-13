using CURSOVA.Application.Abstractions.Persistence;
using CURSOVA.Application.Common;
using CURSOVA.Domain.Comments;
using Microsoft.EntityFrameworkCore;

namespace CURSOVA.Infrastructure.Persistence.Repositories;

public class CommentRepository(MagazineDbContext db) : Repository<Comment>(db), ICommentRepository
{
    public async Task<IReadOnlyList<Comment>> ListForArticleAsync(int articleId, int? viewerId, CancellationToken ct = default) =>
        await Set.AsNoTracking()
            .Where(c => c.ArticleId == articleId
                        && (c.Status == CommentStatus.Published || (c.Status == CommentStatus.Pending && c.UserId == viewerId)))
            .OrderBy(c => c.CreatedAt)
            .ToListAsync(ct);

    public Task<int> CountByUserSinceAsync(int userId, DateTime since, CancellationToken ct = default) =>
        Set.CountAsync(c => c.UserId == userId && c.CreatedAt >= since, ct);

    public async Task<IReadOnlyList<string>> GetRecentBodiesByUserAsync(int userId, int take, CancellationToken ct = default) =>
        await Set.AsNoTracking()
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => c.Body)
            .Take(take)
            .ToListAsync(ct);

    public async Task<PagedResult<Comment>> ListForModerationAsync(CommentStatus? status, int page, int pageSize, CancellationToken ct = default)
    {
        var query = Set.AsNoTracking().Where(c => status == null || c.Status == status);
        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
        return new PagedResult<Comment>(items, page, pageSize, total);
    }

    public Task<int> CountPendingAsync(CancellationToken ct = default) =>
        Set.CountAsync(c => c.Status == CommentStatus.Pending, ct);
}
