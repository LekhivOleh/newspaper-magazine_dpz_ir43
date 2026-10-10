using CURSOVA.Domain.Common;

namespace CURSOVA.Domain.Authors;

public class Author : Entity
{
    public string FullName { get; private set; } = null!;
    public string? Position { get; private set; }
    public string? Bio { get; private set; }
    public string? AvatarUrl { get; private set; }

    private Author() { }

    public Author(string fullName, string? position = null, string? bio = null, string? avatarUrl = null)
    {
        Update(fullName, position, bio, avatarUrl);
    }

    public void Update(string fullName, string? position, string? bio, string? avatarUrl)
    {
        FullName = Guard.NotEmpty(fullName, "Ім'я автора", 120);
        Position = Guard.Optional(position, "Посада", 80);
        Bio = Guard.Optional(bio, "Біографія", 1000);
        AvatarUrl = Guard.Optional(avatarUrl, "Аватар", 500);
    }
}
