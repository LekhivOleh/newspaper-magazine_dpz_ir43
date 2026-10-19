namespace CURSOVA.Application.Abstractions.Sharing;

/// <summary>What we want to share, in our own terms — adapters translate it for each network.</summary>
public record ShareContent(string Url, string Title, string? Summary, IReadOnlyList<string> Hashtags);

/// <summary>Adapter target: the single interface the application uses for every social network.</summary>
public interface IShareTarget
{
    string Key { get; }
    string DisplayName { get; }
    string BuildShareUrl(ShareContent content);
}

public interface IAppUrlProvider
{
    string ArticleUrl(string slug);
    string QuizUrl(int quizId);
    string Absolute(string localPath);
}
