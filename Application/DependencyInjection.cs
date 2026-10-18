using CURSOVA.Application.Articles;
using CURSOVA.Application.Authors;
using CURSOVA.Application.Categories;
using CURSOVA.Application.Comments;
using CURSOVA.Application.Comments.Moderation;
using CURSOVA.Application.Interactive;
using CURSOVA.Application.Issues;
using CURSOVA.Application.Profiles;
using CURSOVA.Application.Sharing;

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

        return services;
    }
}
