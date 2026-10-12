using CURSOVA.Application.Abstractions.Identity;
using CURSOVA.Application.Abstractions.Persistence;
using CURSOVA.Application.Common;
using CURSOVA.Domain.Interactive;
using CURSOVA.Domain.Interactive.Polls;

namespace CURSOVA.Application.Interactive;

public interface IPollService
{
    Task<IReadOnlyList<PollDto>> ListAsync(bool activeOnly, CancellationToken ct = default);
    Task<PollDto> GetAsync(int id, CancellationToken ct = default);
    Task<PollDto?> GetForArticleAsync(int articleId, CancellationToken ct = default);
    Task<PollDto> VoteAsync(int id, IReadOnlyList<int> optionIds, CancellationToken ct = default);

    Task<PollDto> CreateAsync(SavePollRequest request, CancellationToken ct = default);
    Task<PollDto> UpdateAsync(int id, SavePollRequest request, CancellationToken ct = default);
    Task SetActiveAsync(int id, bool active, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}

public class PollService(IUnitOfWork uow, IParticipantContext participant) : IPollService
{
    public async Task<IReadOnlyList<PollDto>> ListAsync(bool activeOnly, CancellationToken ct = default)
    {
        var polls = await uow.Interactive.ListPollsAsync(activeOnly, ct);
        var result = new List<PollDto>(polls.Count);
        foreach (var poll in polls)
            result.Add(await ToDtoAsync(poll, ct));
        return result;
    }

    public async Task<PollDto> GetAsync(int id, CancellationToken ct = default) =>
        await ToDtoAsync(await LoadAsync(id, ct), ct);

    public async Task<PollDto?> GetForArticleAsync(int articleId, CancellationToken ct = default) =>
        await uow.Interactive.GetActivePollForArticleAsync(articleId, ct) is { } poll ? await ToDtoAsync(poll, ct) : null;

    public async Task<PollDto> VoteAsync(int id, IReadOnlyList<int> optionIds, CancellationToken ct = default)
    {
        var poll = await LoadAsync(id, ct);
        var previous = await uow.Interactive.GetLatestSubmissionAsync(id, participant.Key, ct);

        var submission = poll.Submit(new Participant(participant.Key, participant.UserId, previous is not null), optionIds);
        uow.Interactive.AddSubmission(submission);
        await uow.SaveChangesAsync(ct);

        return poll.ToDto(submission.SelectedOptionIds);
    }

    public async Task<PollDto> CreateAsync(SavePollRequest request, CancellationToken ct = default)
    {
        await EnsureArticleExistsAsync(request.ArticleId, ct);
        var poll = new Poll(request.Title, request.Question, request.Options, request.AllowMultiple, request.Description, request.ArticleId);

        uow.Interactive.Add(poll);
        await uow.SaveChangesAsync(ct);
        return poll.ToDto();
    }

    public async Task<PollDto> UpdateAsync(int id, SavePollRequest request, CancellationToken ct = default)
    {
        var poll = await LoadAsync(id, ct);
        await EnsureArticleExistsAsync(request.ArticleId, ct);

        poll.UpdateDetails(request.Title, request.Description, request.ArticleId);
        poll.UpdateQuestion(request.Question, request.Options, request.AllowMultiple);

        await uow.SaveChangesAsync(ct);
        return poll.ToDto();
    }

    public async Task SetActiveAsync(int id, bool active, CancellationToken ct = default)
    {
        var poll = await LoadAsync(id, ct);
        if (active)
            poll.Open();
        else
            poll.Close();
        await uow.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        uow.Interactive.Remove(await LoadAsync(id, ct));
        await uow.SaveChangesAsync(ct);
    }

    private async Task<Poll> LoadAsync(int id, CancellationToken ct) =>
        await uow.Interactive.GetPollAsync(id, ct) ?? throw NotFoundException.For<Poll>(id);

    private async Task<PollDto> ToDtoAsync(Poll poll, CancellationToken ct)
    {
        var mine = await uow.Interactive.GetLatestSubmissionAsync(poll.Id, participant.Key, ct);
        return poll.ToDto(mine?.SelectedOptionIds);
    }

    private async Task EnsureArticleExistsAsync(int? articleId, CancellationToken ct)
    {
        if (articleId is > 0 && await uow.Articles.GetByIdAsync(articleId.Value, ct) is null)
            throw NotFoundException.For<Domain.Articles.Article>(articleId);
    }
}
