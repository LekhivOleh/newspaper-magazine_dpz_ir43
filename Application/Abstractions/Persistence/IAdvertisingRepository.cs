using CURSOVA.Domain.Advertising;

namespace CURSOVA.Application.Abstractions.Persistence;

public interface IAdvertisingRepository
{
    Task<IReadOnlyList<Partner>> ListPartnersAsync(CancellationToken ct = default);
    Task<Partner?> GetPartnerAsync(int id, CancellationToken ct = default);
    Task<bool> PartnerIsInUseAsync(int id, CancellationToken ct = default);
    void AddPartner(Partner partner);
    void RemovePartner(Partner partner);

    Task<IReadOnlyList<Ad>> ListAdsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Ad>> ListActiveAdsAsync(CancellationToken ct = default);
    Task<Ad?> GetAdAsync(int id, CancellationToken ct = default);
    void AddAd(Ad ad);
    void RemoveAd(Ad ad);
}
