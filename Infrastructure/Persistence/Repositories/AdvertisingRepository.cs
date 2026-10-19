using CURSOVA.Application.Abstractions.Persistence;
using CURSOVA.Domain.Advertising;
using Microsoft.EntityFrameworkCore;

namespace CURSOVA.Infrastructure.Persistence.Repositories;

public class AdvertisingRepository(MagazineDbContext db) : IAdvertisingRepository
{
    public async Task<IReadOnlyList<Partner>> ListPartnersAsync(CancellationToken ct = default) =>
        await db.Partners.OrderBy(p => p.Name).ToListAsync(ct);

    public Task<Partner?> GetPartnerAsync(int id, CancellationToken ct = default) =>
        db.Partners.FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<bool> PartnerIsInUseAsync(int id, CancellationToken ct = default) =>
        await db.Ads.AnyAsync(a => a.PartnerId == id, ct) || await db.Articles.AnyAsync(a => a.SponsorPartnerId == id, ct);

    public void AddPartner(Partner partner) => db.Partners.Add(partner);

    public void RemovePartner(Partner partner) => db.Partners.Remove(partner);

    public async Task<IReadOnlyList<Ad>> ListAdsAsync(CancellationToken ct = default) =>
        await db.Ads.OrderByDescending(a => a.IsActive).ThenBy(a => a.Headline).ToListAsync(ct);

    public async Task<IReadOnlyList<Ad>> ListActiveAdsAsync(CancellationToken ct = default) =>
        await db.Ads.Where(a => a.IsActive).ToListAsync(ct);

    public Task<Ad?> GetAdAsync(int id, CancellationToken ct = default) => db.Ads.FirstOrDefaultAsync(a => a.Id == id, ct);

    public void AddAd(Ad ad) => db.Ads.Add(ad);

    public void RemoveAd(Ad ad) => db.Ads.Remove(ad);
}
