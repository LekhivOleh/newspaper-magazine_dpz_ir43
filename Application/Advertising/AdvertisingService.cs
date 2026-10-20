using CURSOVA.Application.Abstractions.Persistence;
using CURSOVA.Application.Common;
using CURSOVA.Application.Notifications;
using CURSOVA.Domain.Advertising;

namespace CURSOVA.Application.Advertising;

public record PartnerDto(int Id, string Name, string? Tagline, string? LogoUrl, string WebsiteUrl, bool IsActive);

public record AdDto(
    int Id,
    string Headline,
    string? Body,
    string? ImageUrl,
    string TargetUrl,
    string CallToAction,
    int? PartnerId,
    string? PartnerName,
    int? CategoryId,
    bool IsActive,
    int Impressions,
    int Clicks,
    DateTime? PromotedAt = null)
{
    public double ClickThroughRate => Impressions == 0 ? 0 : (double)Clicks / Impressions;
}

public record SavePartnerRequest(string Name, string WebsiteUrl, string? Tagline, string? LogoUrl, bool IsActive);

public record SaveAdRequest(string Headline, string TargetUrl, string CallToAction, string? Body, string? ImageUrl,
    int? PartnerId, int? CategoryId, bool IsActive);

public interface IAdvertisingService
{
    /// <summary>Picks ads for a page and counts the impressions.</summary>
    Task<IReadOnlyList<AdDto>> PickAsync(int? categoryId, int count, int? partnerId = null, CancellationToken ct = default);
    Task<string> ClickAsync(int adId, CancellationToken ct = default);
    Task<PartnerDto?> FindActivePartnerAsync(int partnerId, CancellationToken ct = default);

    Task<IReadOnlyList<PartnerDto>> ListPartnersAsync(CancellationToken ct = default);
    Task<PartnerDto> GetPartnerAsync(int id, CancellationToken ct = default);
    Task SavePartnerAsync(int? id, SavePartnerRequest request, CancellationToken ct = default);
    Task DeletePartnerAsync(int id, CancellationToken ct = default);

    Task<IReadOnlyList<AdDto>> ListAdsAsync(CancellationToken ct = default);
    Task<AdDto> GetAdAsync(int id, CancellationToken ct = default);
    Task SaveAdAsync(int? id, SaveAdRequest request, CancellationToken ct = default);
    Task DeleteAdAsync(int id, CancellationToken ct = default);

    /// <summary>Announces an ad to free readers as a partner promotion notification.</summary>
    Task LaunchPromotionAsync(int adId, CancellationToken ct = default);
}

public class AdvertisingService(IUnitOfWork uow, IMagazineEventPublisher events, TimeProvider clock) : IAdvertisingService
{
    public async Task<IReadOnlyList<AdDto>> PickAsync(int? categoryId, int count, int? partnerId = null, CancellationToken ct = default)
    {
        var partners = (await uow.Advertising.ListPartnersAsync(ct)).ToDictionary(p => p.Id);
        var eligible = (await uow.Advertising.ListActiveAdsAsync(ct))
            .Where(a => categoryId is null || a.TargetsCategory(categoryId.Value))
            .Where(a => partnerId is null || a.PartnerId == partnerId)
            .Where(a => a.PartnerId is null || partners.GetValueOrDefault(a.PartnerId.Value)?.IsActive == true)
            .ToList();

        // Category-targeted ads first, then rotate so every campaign gets its share of impressions.
        var picked = eligible
            .OrderByDescending(a => a.CategoryId is not null)
            .ThenBy(a => a.Impressions)
            .ThenBy(_ => Random.Shared.Next())
            .Take(count)
            .ToList();

        foreach (var ad in picked)
            ad.RegisterImpression();
        if (picked.Count > 0)
            await uow.SaveChangesAsync(ct);

        return picked.Select(a => ToDto(a, partners)).ToList();
    }

    public async Task<string> ClickAsync(int adId, CancellationToken ct = default)
    {
        var ad = await uow.Advertising.GetAdAsync(adId, ct) ?? throw NotFoundException.For<Ad>(adId);
        ad.RegisterClick();
        await uow.SaveChangesAsync(ct);
        return ad.TargetUrl;
    }

    public async Task<PartnerDto?> FindActivePartnerAsync(int partnerId, CancellationToken ct = default) =>
        await uow.Advertising.GetPartnerAsync(partnerId, ct) is { IsActive: true } p ? ToDto(p) : null;

    public async Task<IReadOnlyList<PartnerDto>> ListPartnersAsync(CancellationToken ct = default) =>
        (await uow.Advertising.ListPartnersAsync(ct)).Select(ToDto).ToList();

    public async Task<PartnerDto> GetPartnerAsync(int id, CancellationToken ct = default) =>
        ToDto(await uow.Advertising.GetPartnerAsync(id, ct) ?? throw NotFoundException.For<Partner>(id));

    public async Task SavePartnerAsync(int? id, SavePartnerRequest r, CancellationToken ct = default)
    {
        if (id is null)
        {
            var partner = new Partner(r.Name, r.WebsiteUrl, r.Tagline, r.LogoUrl);
            partner.Update(r.Name, r.WebsiteUrl, r.Tagline, r.LogoUrl, r.IsActive);
            uow.Advertising.AddPartner(partner);
        }
        else
        {
            var partner = await uow.Advertising.GetPartnerAsync(id.Value, ct) ?? throw NotFoundException.For<Partner>(id);
            partner.Update(r.Name, r.WebsiteUrl, r.Tagline, r.LogoUrl, r.IsActive);
        }
        await uow.SaveChangesAsync(ct);
    }

    public async Task DeletePartnerAsync(int id, CancellationToken ct = default)
    {
        if (await uow.Advertising.PartnerIsInUseAsync(id, ct))
            throw new ConflictException("Партнер має оголошення або спонсоровані статті — спершу вимкніть його.");

        uow.Advertising.RemovePartner(await uow.Advertising.GetPartnerAsync(id, ct) ?? throw NotFoundException.For<Partner>(id));
        await uow.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<AdDto>> ListAdsAsync(CancellationToken ct = default)
    {
        var partners = (await uow.Advertising.ListPartnersAsync(ct)).ToDictionary(p => p.Id);
        return (await uow.Advertising.ListAdsAsync(ct)).Select(a => ToDto(a, partners)).ToList();
    }

    public async Task<AdDto> GetAdAsync(int id, CancellationToken ct = default)
    {
        var partners = (await uow.Advertising.ListPartnersAsync(ct)).ToDictionary(p => p.Id);
        return ToDto(await uow.Advertising.GetAdAsync(id, ct) ?? throw NotFoundException.For<Ad>(id), partners);
    }

    public async Task SaveAdAsync(int? id, SaveAdRequest r, CancellationToken ct = default)
    {
        if (r.PartnerId is > 0 && await uow.Advertising.GetPartnerAsync(r.PartnerId.Value, ct) is null)
            throw NotFoundException.For<Partner>(r.PartnerId);

        if (id is null)
        {
            var ad = new Ad(r.Headline, r.TargetUrl, r.CallToAction, r.Body, r.ImageUrl, r.PartnerId, r.CategoryId);
            ad.Update(r.Headline, r.TargetUrl, r.CallToAction, r.Body, r.ImageUrl, r.PartnerId, r.CategoryId, r.IsActive);
            uow.Advertising.AddAd(ad);
        }
        else
        {
            var ad = await uow.Advertising.GetAdAsync(id.Value, ct) ?? throw NotFoundException.For<Ad>(id);
            ad.Update(r.Headline, r.TargetUrl, r.CallToAction, r.Body, r.ImageUrl, r.PartnerId, r.CategoryId, r.IsActive);
        }
        await uow.SaveChangesAsync(ct);
    }

    public async Task DeleteAdAsync(int id, CancellationToken ct = default)
    {
        uow.Advertising.RemoveAd(await uow.Advertising.GetAdAsync(id, ct) ?? throw NotFoundException.For<Ad>(id));
        await uow.SaveChangesAsync(ct);
    }

    public async Task LaunchPromotionAsync(int adId, CancellationToken ct = default)
    {
        var ad = await uow.Advertising.GetAdAsync(adId, ct) ?? throw NotFoundException.For<Ad>(adId);
        var partner = ad.PartnerId is { } partnerId ? await uow.Advertising.GetPartnerAsync(partnerId, ct) : null;
        if (partner is { IsActive: false })
            throw new ConflictException("Партнер цього оголошення вимкнений.");

        ad.MarkPromoted(clock.GetUtcNow().UtcDateTime);
        await uow.SaveChangesAsync(ct);

        await events.PublishAsync(new PartnerPromotionLaunched(ad.Id, ad.PartnerId, partner?.Name, ad.Headline, ad.Body, ad.ImageUrl), ct);
    }

    private static PartnerDto ToDto(Partner p) => new(p.Id, p.Name, p.Tagline, p.LogoUrl, p.WebsiteUrl, p.IsActive);

    private static AdDto ToDto(Ad a, IReadOnlyDictionary<int, Partner> partners) => new(
        a.Id, a.Headline, a.Body, a.ImageUrl, a.TargetUrl, a.CallToAction, a.PartnerId,
        a.PartnerId is { } pid ? partners.GetValueOrDefault(pid)?.Name : null,
        a.CategoryId, a.IsActive, a.Impressions, a.Clicks, a.PromotedAt);
}
