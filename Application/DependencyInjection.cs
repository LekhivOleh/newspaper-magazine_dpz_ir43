using CURSOVA.Application.Articles;
using CURSOVA.Application.Authors;
using CURSOVA.Application.Categories;
using CURSOVA.Application.Comments;
using CURSOVA.Application.Comments.Moderation;
using CURSOVA.Application.Interactive;
using CURSOVA.Application.Issues;
using CURSOVA.Application.Profiles;
using CURSOVA.Application.Recommendations;
using CURSOVA.Application.Sharing;
using CURSOVA.Application.Subscriptions;

namespace CURSOVA.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IArticleService, ArticleService>();
        services.AddScoped<IAuthorService, AuthorService>();
        services.AddScoped<IIssueService, IssueService>();
        services.AddScoped<IPollService, PollService>();
        services.AddScoped<IQuizService, QuizService>();
        services.AddScoped<ICommentService, CommentService>();
        services.AddSingleton<ICommentModerator, ModerationPipeline>();
        services.AddScoped<ISharingFacade, SharingFacade>();
        services.AddScoped<ISavedArticleService, SavedArticleService>();
        services.AddScoped<ICollectionService, CollectionService>();
        services.AddRecommendations();
        services.AddScoped<ISubscriptionService, SubscriptionService>();

        return services;
    }

    private static void AddRecommendations(this IServiceCollection services)
    {
        services.AddSingleton<InterestBasedStrategy>();
        services.AddSingleton<PopularityStrategy>();
        services.AddSingleton(sp => new HybridStrategy(
            (sp.GetRequiredService<InterestBasedStrategy>(), 0.7),
            (sp.GetRequiredService<PopularityStrategy>(), 0.3)));

        services.AddSingleton<IRecommendationStrategy>(sp => sp.GetRequiredService<HybridStrategy>());
        services.AddSingleton<IRecommendationStrategy>(sp => sp.GetRequiredService<InterestBasedStrategy>());
        services.AddSingleton<IRecommendationStrategy>(sp => sp.GetRequiredService<PopularityStrategy>());

        services.AddScoped<IRecommendationService, RecommendationService>();
    }
}
