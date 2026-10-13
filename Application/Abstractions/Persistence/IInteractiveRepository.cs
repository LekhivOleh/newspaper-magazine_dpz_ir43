using CURSOVA.Domain.Interactive;
using CURSOVA.Domain.Interactive.Polls;
using CURSOVA.Domain.Interactive.Quizzes;

namespace CURSOVA.Application.Abstractions.Persistence;

public interface IInteractiveRepository : IRepository<InteractiveElement>
{
    Task<Poll?> GetPollAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<Poll>> ListPollsAsync(bool activeOnly, CancellationToken ct = default);
    Task<Poll?> GetActivePollForArticleAsync(int articleId, CancellationToken ct = default);

    Task<Quiz?> GetQuizAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<Quiz>> ListQuizzesAsync(bool activeOnly, CancellationToken ct = default);

    Task<Submission?> GetLatestSubmissionAsync(int elementId, string participantKey, CancellationToken ct = default);
    void AddSubmission(Submission submission);
}
