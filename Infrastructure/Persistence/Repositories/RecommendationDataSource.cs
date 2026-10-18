using CURSOVA.Application.Abstractions.Persistence;
using CURSOVA.Application.Recommendations;
using CURSOVA.Domain.Articles;
using CURSOVA.Domain.Comments;
using CURSOVA.Domain.Profiles;
using CURSOVA.Domain.Sharing;
using Microsoft.EntityFrameworkCore;

namespace CURSOVA.Infrastructure.Persistence.Repositories;

public class RecommendationDataSource(MagazineDbContext db) : IRecommendationDataSource
{
    public async Task<IReadOnlyList<ArticleCandidate>> GetCandidatesAsync(int max, CancellationToken ct = default) =>
        await db.Articles.AsNoTracking()
            .Where(a => a.Status == ArticleStatus.Published)
            .OrderByDescending(a => a.PublishedAt)
            .Take(max)
            .Select(a => new ArticleCandidate(
                a.Id,
                a.CategoryId,
                a.Category.Name,
                a.PublishedAt ?? a.CreatedAt,
                a.ViewCount,
                db.ShareEvents.Count(s => s.ContentType == ShareableType.Article && s.ContentId == a.Id),
                db.Comments.Count(c => c.ArticleId == a.Id && c.Status == CommentStatus.Published)))
            .ToListAsync(ct);

    public async Task<InterestSignals> GetSignalsAsync(int userId, CancellationToken ct = default)
    {
        var saved = await CountByCategoryAsync(db.SavedArticles.Where(s => s.UserId == userId).Select(s => s.ArticleId), ct);
        var collected = await CountByCategoryAsync(
            db.Collections.Where(c => c.UserId == userId).SelectMany(c => c.Items).Select(i => i.ArticleId), ct);
        var commented = await CountByCategoryAsync(db.Comments.Where(c => c.UserId == userId).Select(c => c.ArticleId), ct);

        return new InterestSignals(await GetExplicitInterestsAsync(userId, ct), saved, collected, commented);
    }

    public async Task<IReadOnlySet<int>> GetKeptArticleIdsAsync(int userId, CancellationToken ct = default)
    {
        var saved = await db.SavedArticles.Where(s => s.UserId == userId).Select(s => s.ArticleId).ToListAsync(ct);
        var collected = await db.Collections.Where(c => c.UserId == userId)
            .SelectMany(c => c.Items).Select(i => i.ArticleId).ToListAsync(ct);
        return saved.Concat(collected).ToHashSet();
    }

    public async Task<IReadOnlyList<int>> GetExplicitInterestsAsync(int userId, CancellationToken ct = default) =>
        await db.UserInterests.Where(i => i.UserId == userId).Select(i => i.CategoryId).ToListAsync(ct);

    public async Task SetExplicitInterestsAsync(int userId, IReadOnlyCollection<int> categoryIds, CancellationToken ct = default)
    {
        db.UserInterests.RemoveRange(db.UserInterests.Where(i => i.UserId == userId));
        db.UserInterests.AddRange(categoryIds.Distinct().Select(id => new UserInterest(userId, id)));
        await db.SaveChangesAsync(ct);
    }

    private async Task<IReadOnlyDictionary<int, int>> CountByCategoryAsync(IQueryable<int> articleIds, CancellationToken ct) =>
        await db.Articles
            .Where(a => articleIds.Contains(a.Id))
            .GroupBy(a => a.CategoryId)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count, ct);
}
