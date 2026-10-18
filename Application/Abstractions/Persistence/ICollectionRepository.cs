using CURSOVA.Domain.Profiles;

namespace CURSOVA.Application.Abstractions.Persistence;

public interface ICollectionRepository : IRepository<Collection>
{
    Task<Collection?> GetWithItemsAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<Collection>> ListByUserAsync(int userId, CancellationToken ct = default);
    Task<IReadOnlyList<Collection>> ListPublicAsync(int take, CancellationToken ct = default);
}
