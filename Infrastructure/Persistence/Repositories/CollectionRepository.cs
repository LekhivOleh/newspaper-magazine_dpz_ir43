using CURSOVA.Application.Abstractions.Persistence;
using CURSOVA.Domain.Profiles;
using Microsoft.EntityFrameworkCore;

namespace CURSOVA.Infrastructure.Persistence.Repositories;

public class CollectionRepository(MagazineDbContext db) : Repository<Collection>(db), ICollectionRepository
{
    private IQueryable<Collection> WithItems => Set.Include(c => c.Items);

    public Task<Collection?> GetWithItemsAsync(int id, CancellationToken ct = default) =>
        WithItems.FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<IReadOnlyList<Collection>> ListByUserAsync(int userId, CancellationToken ct = default) =>
        await WithItems.AsNoTracking().Where(c => c.UserId == userId).OrderByDescending(c => c.CreatedAt).ToListAsync(ct);

    public async Task<IReadOnlyList<Collection>> ListPublicAsync(int take, CancellationToken ct = default) =>
        await WithItems.AsNoTracking()
            .Where(c => c.IsPublic && c.Items.Any())
            .OrderByDescending(c => c.CreatedAt)
            .Take(take)
            .ToListAsync(ct);
}
