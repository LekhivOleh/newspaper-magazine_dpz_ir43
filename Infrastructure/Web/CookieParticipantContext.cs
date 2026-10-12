using CURSOVA.Application.Abstractions.Identity;

namespace CURSOVA.Infrastructure.Web;

public class CookieParticipantContext(IHttpContextAccessor accessor, ICurrentUser currentUser) : IParticipantContext
{
    private const string CookieName = "tp_participant";

    private string? _anonymousKey;

    public int? UserId => currentUser.Id;

    public string Key => currentUser.Id is { } id ? $"user:{id}" : $"anon:{AnonymousId()}";

    private string AnonymousId()
    {
        if (_anonymousKey is not null)
            return _anonymousKey;

        var context = accessor.HttpContext ?? throw new InvalidOperationException("Немає HTTP-контексту.");
        if (context.Request.Cookies.TryGetValue(CookieName, out var existing) && Guid.TryParse(existing, out _))
            return _anonymousKey = existing;

        _anonymousKey = Guid.NewGuid().ToString("N");
        context.Response.Cookies.Append(CookieName, _anonymousKey, new CookieOptions
        {
            HttpOnly = true,
            IsEssential = true,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddYears(1)
        });
        return _anonymousKey;
    }
}
