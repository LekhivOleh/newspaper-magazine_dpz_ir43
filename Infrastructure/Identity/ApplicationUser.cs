using Microsoft.AspNetCore.Identity;

namespace CURSOVA.Infrastructure.Identity;

public class ApplicationUser : IdentityUser<int>
{
    public string DisplayName { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
