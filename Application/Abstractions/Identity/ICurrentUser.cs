namespace CURSOVA.Application.Abstractions.Identity;

public interface ICurrentUser
{
    int? Id { get; }
    bool IsAuthenticated { get; }
    bool IsAdmin { get; }
    string? DisplayName { get; }
}
