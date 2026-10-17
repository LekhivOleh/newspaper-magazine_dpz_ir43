using CURSOVA.Application.Abstractions.Persistence;
using CURSOVA.Domain.Sharing;
using Microsoft.EntityFrameworkCore;

namespace CURSOVA.Infrastructure.Persistence.Repositories;

public class ShareRepository(MagazineDbContext db) : IShareRepository
{
    public void Add(ShareEvent shareEvent) => db.ShareEvents.Add(shareEvent);

    public async Task<IReadOnlyDictionary<string, int>> CountByNetworkAsync(ShareableType type, int contentId, CancellationToken ct = default) =>
        await db.ShareEvents
            .Where(s => s.ContentType == type && s.ContentId == contentId)
            .GroupBy(s => s.Network)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count, ct);

    public async Task<IReadOnlyList<(int ContentId, int Count)>> TopAsync(ShareableType type, int take, CancellationToken ct = default) =>
        (await db.ShareEvents
            .Where(s => s.ContentType == type)
            .GroupBy(s => s.ContentId)
            .Select(g => new { g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(take)
            .ToListAsync(ct))
        .Select(x => (x.Key, x.Count))
        .ToList();
}
