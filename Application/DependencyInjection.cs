using CURSOVA.Application.Articles;
using CURSOVA.Application.Categories;

namespace CURSOVA.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IArticleService, ArticleService>();

        return services;
    }
}
