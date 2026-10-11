using CURSOVA.Application.Abstractions.Identity;
using CURSOVA.Application.Abstractions.Persistence;
using CURSOVA.Infrastructure.Identity;
using CURSOVA.Infrastructure.Persistence;
using CURSOVA.Infrastructure.Persistence.Repositories;
using CURSOVA.Infrastructure.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CURSOVA.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<MagazineDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("Magazine")));

        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IAuthorRepository, AuthorRepository>();
        services.AddScoped<IArticleRepository, ArticleRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddIdentityServices();

        services.AddProblemDetails();
        services.AddExceptionHandler<ApiExceptionHandler>();

        return services;
    }

    private static void AddIdentityServices(this IServiceCollection services)
    {
        services.AddIdentity<ApplicationUser, IdentityRole<int>>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
            })
            .AddEntityFrameworkStores<MagazineDbContext>()
            .AddDefaultTokenProviders()
            .AddErrorDescriber<UkrainianIdentityErrorDescriber>();

        services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/account/login";
            options.AccessDeniedPath = "/account/access-denied";
            options.SlidingExpiration = true;
            options.ExpireTimeSpan = TimeSpan.FromDays(14);

            options.Events.OnRedirectToLogin = context => ApiAwareRedirect(context, StatusCodes.Status401Unauthorized);
            options.Events.OnRedirectToAccessDenied = context => ApiAwareRedirect(context, StatusCodes.Status403Forbidden);
        });

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, HttpCurrentUser>();
    }

    private static Task ApiAwareRedirect(RedirectContext<CookieAuthenticationOptions> context, int apiStatusCode)
    {
        if (context.Request.Path.StartsWithSegments("/api"))
            context.Response.StatusCode = apiStatusCode;
        else
            context.Response.Redirect(context.RedirectUri);

        return Task.CompletedTask;
    }
}
