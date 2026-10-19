using CURSOVA.Application.Abstractions.Identity;
using CURSOVA.Application.Advertising;
using CURSOVA.Application.Articles;
using CURSOVA.Application.Articles.Access;
using CURSOVA.Application.Articles.Presentation;
using CURSOVA.Application.Authors;
using CURSOVA.Application.Categories;
using CURSOVA.Application.Comments;
using CURSOVA.Application.Comments.Moderation;
using CURSOVA.Application.Interactive;
using CURSOVA.Application.Issues;
using CURSOVA.Application.Notifications;
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
        services.AddScoped<ArticleReader>();
        services.AddScoped<IArticleReader>(sp => new PremiumContentProxy(
            sp.GetRequiredService<ArticleReader>(),
            sp.GetRequiredService<ISubscriptionService>(),
            sp.GetRequiredService<ICurrentUser>()));
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
        services.AddScoped<IAdvertisingService, AdvertisingService>();

        // Decorator chain for the article page: sponsor layer around ad slots around the plain article.
        services.AddScoped<IArticlePageComposer>(sp =>
        {
            var ads = sp.GetRequiredService<IAdvertisingService>();
            return new SponsoredContentDecorator(new AdSlotsDecorator(new PlainArticleComposer(), ads), ads);
        });

        services.AddNotifications();

        return services;
    }

    private static void AddNotifications(this IServiceCollection services)
    {
        services.AddSingleton<NotificationFactory, NewIssueNotificationFactory>();
        services.AddSingleton<NotificationFactory, ExclusiveContentNotificationFactory>();
        services.AddSingleton<NotificationFactory, PartnerPromoNotificationFactory>();
        services.AddSingleton<INotificationFactories, NotificationFactories>();

        services.AddScoped<IAudienceResolver, AudienceResolver>();
        services.AddScoped<IMagazineEventObserver, InAppNotificationObserver>();
        services.AddScoped<IMagazineEventObserver, EmailNotificationObserver>();
        services.AddScoped<IMagazineEventPublisher, MagazineEventPublisher>();

        services.AddScoped<INotificationService, NotificationService>();
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
