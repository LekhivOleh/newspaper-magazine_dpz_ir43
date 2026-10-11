using CURSOVA.Application.Abstractions.Persistence;
using CURSOVA.Infrastructure.Persistence;
using CURSOVA.Infrastructure.Persistence.Repositories;
using CURSOVA.Infrastructure.Web;
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

        services.AddProblemDetails();
        services.AddExceptionHandler<ApiExceptionHandler>();

        return services;
    }
}
