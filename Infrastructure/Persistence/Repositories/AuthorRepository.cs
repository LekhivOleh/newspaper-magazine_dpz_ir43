using CURSOVA.Application.Abstractions.Persistence;
using CURSOVA.Domain.Authors;
using Microsoft.EntityFrameworkCore;

namespace CURSOVA.Infrastructure.Persistence.Repositories;

public class AuthorRepository(MagazineDbContext db) : Repository<Author>(db), IAuthorRepository
{
    public override async Task<IReadOnlyList<Author>> ListAsync(CancellationToken ct = default) =>
        await Set.AsNoTracking().OrderBy(a => a.FullName).ToListAsync(ct);
}
