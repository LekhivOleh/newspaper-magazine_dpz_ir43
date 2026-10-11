using CURSOVA.Application.Abstractions.Identity;
using Microsoft.AspNetCore.Identity;

namespace CURSOVA.Infrastructure.Identity;

public static class IdentitySeeder
{
    public static async Task InitializeAsync(IServiceProvider services, IConfiguration configuration)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole<int>>>();
        foreach (var role in Roles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole<int>(role));
        }

        var email = configuration["SeedAdmin:Email"];
        var password = configuration["SeedAdmin:Password"];
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return;

        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        if (await userManager.FindByEmailAsync(email) is not null)
            return;

        var admin = new ApplicationUser { UserName = email, Email = email, DisplayName = "Редакція TechPulse", EmailConfirmed = true };
        var result = await userManager.CreateAsync(admin, password);
        if (!result.Succeeded)
            throw new InvalidOperationException(
                "Не вдалося створити адміністратора: " + string.Join("; ", result.Errors.Select(e => e.Description)));

        await userManager.AddToRolesAsync(admin, Roles.All);
    }
}
