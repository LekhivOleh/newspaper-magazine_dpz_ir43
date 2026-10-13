using System.Security.Claims;
using CURSOVA.Application.Abstractions.Identity;

namespace CURSOVA.Infrastructure.Identity;

public class HttpCurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    private ClaimsPrincipal? Principal => accessor.HttpContext?.User;

    public int? Id => int.TryParse(Principal?.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated == true;

    public bool IsAdmin => Principal?.IsInRole(Roles.Admin) == true;

    public string? DisplayName =>
        Principal?.FindFirstValue(AppClaimsPrincipalFactory.DisplayNameClaim) ?? Principal?.Identity?.Name;
}
