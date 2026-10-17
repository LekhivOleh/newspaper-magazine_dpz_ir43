using CURSOVA.Application.Abstractions.Sharing;

namespace CURSOVA.Infrastructure.Web;

public class AppUrlProvider(IHttpContextAccessor accessor, IConfiguration configuration) : IAppUrlProvider
{
    public string ArticleUrl(string slug) => $"{BaseUrl}/article/{Uri.EscapeDataString(slug)}";

    public string QuizUrl(int quizId) => $"{BaseUrl}/quizzes/{quizId}";

    // Configurable for deployments behind a proxy; otherwise taken from the current request.
    private string BaseUrl =>
        configuration["PublicBaseUrl"]?.TrimEnd('/')
        ?? (accessor.HttpContext?.Request is { } request
            ? $"{request.Scheme}://{request.Host}{request.PathBase}"
            : "http://localhost");
}
