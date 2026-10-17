using CURSOVA.Domain.Sharing;

namespace CURSOVA.Application.Abstractions.Persistence;

public interface IShareRepository
{
    void Add(ShareEvent shareEvent);
    Task<IReadOnlyDictionary<string, int>> CountByNetworkAsync(ShareableType type, int contentId, CancellationToken ct = default);
    Task<IReadOnlyList<(int ContentId, int Count)>> TopAsync(ShareableType type, int take, CancellationToken ct = default);
}
