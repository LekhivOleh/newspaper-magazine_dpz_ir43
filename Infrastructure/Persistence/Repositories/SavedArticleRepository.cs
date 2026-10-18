using CURSOVA.Application.Abstractions.Persistence;
using CURSOVA.Application.Common;
using CURSOVA.Domain.Articles;
using CURSOVA.Domain.Profiles;
using Microsoft.EntityFrameworkCore;

namespace CURSOVA.Infrastructure.Persistence.Repositories;

public class SavedArticleRepository(MagazineDbContext db) : ISavedArticleRepository
{
    public Task<SavedArticle?> FindAsync(int userId, int articleId, CancellationToken ct = default) =>
        db.SavedArticles.FirstOrDefaultAsync(s => s.UserId == userId && s.ArticleId == articleId, ct);

    // Only published articles count: drafts/archived ones stay saved but are hidden until republished.
    public async Task<PagedResult<SavedArticle>> ListAsync(int userId, int page, int pageSize, CancellationToken ct = default)
    {
        var query = db.SavedArticles.AsNoTracking()
            .Where(s => s.UserId == userId)
            .Where(s => db.Articles.Any(a => a.Id == s.ArticleId && a.Status == ArticleStatus.Published));

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(s => s.SavedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<SavedArticle>(items, page, pageSize, total);
    }

    public void Add(SavedArticle saved) => db.SavedArticles.Add(saved);

    public void Remove(SavedArticle saved) => db.SavedArticles.Remove(saved);
}
