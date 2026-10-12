using System.ComponentModel.DataAnnotations;
using CURSOVA.Application.Abstractions.Persistence;
using CURSOVA.Application.Articles;
using CURSOVA.Application.Common;
using CURSOVA.Domain.Authors;

namespace CURSOVA.Application.Authors;

public record SaveAuthorRequest(
    [Required, MaxLength(120)] string FullName,
    [MaxLength(80)] string? Position,
    [MaxLength(1000)] string? Bio,
    [MaxLength(500)] string? AvatarUrl);

public interface IAuthorService
{
    Task<IReadOnlyList<AuthorDto>> ListAsync(CancellationToken ct = default);
    Task<AuthorDto> GetAsync(int id, CancellationToken ct = default);
    Task<AuthorDto> CreateAsync(SaveAuthorRequest request, CancellationToken ct = default);
    Task<AuthorDto> UpdateAsync(int id, SaveAuthorRequest request, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}

public class AuthorService(IUnitOfWork uow) : IAuthorService
{
    public async Task<IReadOnlyList<AuthorDto>> ListAsync(CancellationToken ct = default) =>
        (await uow.Authors.ListAsync(ct)).Select(ToDto).ToList();

    public async Task<AuthorDto> GetAsync(int id, CancellationToken ct = default) => ToDto(await LoadAsync(id, ct));

    public async Task<AuthorDto> CreateAsync(SaveAuthorRequest request, CancellationToken ct = default)
    {
        var author = new Author(request.FullName, request.Position, request.Bio, request.AvatarUrl);
        uow.Authors.Add(author);
        await uow.SaveChangesAsync(ct);
        return ToDto(author);
    }

    public async Task<AuthorDto> UpdateAsync(int id, SaveAuthorRequest request, CancellationToken ct = default)
    {
        var author = await LoadAsync(id, ct);
        author.Update(request.FullName, request.Position, request.Bio, request.AvatarUrl);
        await uow.SaveChangesAsync(ct);
        return ToDto(author);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var author = await LoadAsync(id, ct);
        if (await uow.Authors.HasArticlesAsync(id, ct))
            throw new ConflictException("Не можна видалити автора, у якого є статті.");

        uow.Authors.Remove(author);
        await uow.SaveChangesAsync(ct);
    }

    private async Task<Author> LoadAsync(int id, CancellationToken ct) =>
        await uow.Authors.GetByIdAsync(id, ct) ?? throw NotFoundException.For<Author>(id);

    private static AuthorDto ToDto(Author a) => new(a.Id, a.FullName, a.Position, a.Bio, a.AvatarUrl);
}
