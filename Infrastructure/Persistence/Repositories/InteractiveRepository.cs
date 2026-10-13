using CURSOVA.Application.Abstractions.Persistence;
using CURSOVA.Domain.Interactive;
using CURSOVA.Domain.Interactive.Polls;
using CURSOVA.Domain.Interactive.Quizzes;
using Microsoft.EntityFrameworkCore;

namespace CURSOVA.Infrastructure.Persistence.Repositories;

public class InteractiveRepository(MagazineDbContext db) : Repository<InteractiveElement>(db), IInteractiveRepository
{
    private IQueryable<Poll> Polls => Db.Set<Poll>().Include(p => p.Options);

    public Task<Poll?> GetPollAsync(int id, CancellationToken ct = default) =>
        Polls.FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<IReadOnlyList<Poll>> ListPollsAsync(bool activeOnly, CancellationToken ct = default) =>
        await Polls.AsNoTracking()
            .Where(p => !activeOnly || p.IsActive)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(ct);

    public Task<Poll?> GetActivePollForArticleAsync(int articleId, CancellationToken ct = default) =>
        Polls.AsNoTracking()
            .Where(p => p.ArticleId == articleId && p.IsActive)
            .OrderByDescending(p => p.CreatedAt)
            .FirstOrDefaultAsync(ct);

    public Task<Quiz?> GetQuizAsync(int id, CancellationToken ct = default) =>
        Db.Set<Quiz>()
            .Include(q => q.Questions).ThenInclude(q => q.Options)
            .Include(q => q.Outcomes)
            .AsSplitQuery()
            .FirstOrDefaultAsync(q => q.Id == id, ct);

    public async Task<IReadOnlyList<Quiz>> ListQuizzesAsync(bool activeOnly, CancellationToken ct = default) =>
        await Db.Set<Quiz>().AsNoTracking()
            .Include(q => q.Questions)
            .Where(q => !activeOnly || q.IsActive)
            .OrderByDescending(q => q.CreatedAt)
            .ToListAsync(ct);

    public Task<Submission?> GetLatestSubmissionAsync(int elementId, string participantKey, CancellationToken ct = default) =>
        Db.Set<Submission>().AsNoTracking()
            .Where(s => s.ElementId == elementId && s.ParticipantKey == participantKey)
            .OrderByDescending(s => s.SubmittedAt)
            .FirstOrDefaultAsync(ct);

    public void AddSubmission(Submission submission) => Db.Set<Submission>().Add(submission);
}
