using CURSOVA.Domain.Common;

namespace CURSOVA.Application.Abstractions.Persistence;

public interface IRepository<T> where T : Entity
{
    Task<T?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<T>> ListAsync(CancellationToken ct = default);
    void Add(T entity);
    void Remove(T entity);
}
