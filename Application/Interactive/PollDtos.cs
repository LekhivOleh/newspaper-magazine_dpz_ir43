using System.ComponentModel.DataAnnotations;
using CURSOVA.Domain.Interactive.Polls;

namespace CURSOVA.Application.Interactive;

public record PollOptionDto(int Id, string Text, int Votes, int Percent, bool IsMine);

public record PollDto(
    int Id,
    string Title,
    string? Description,
    string Question,
    bool AllowMultiple,
    bool IsActive,
    int? ArticleId,
    int ParticipantCount,
    int TotalVotes,
    bool HasVoted,
    DateTime CreatedAt,
    IReadOnlyList<PollOptionDto> Options)
{
    public bool ShowResults => HasVoted || !IsActive;
}

public record VoteRequest([Required, MinLength(1)] IReadOnlyList<int> OptionIds);

public record SavePollRequest(
    string Title,
    string? Description,
    string Question,
    IReadOnlyList<string> Options,
    bool AllowMultiple,
    int? ArticleId);

public static class PollMapper
{
    public static PollDto ToDto(this Poll poll, IReadOnlyCollection<int>? myOptionIds = null)
    {
        var total = poll.TotalVotes;
        var mine = myOptionIds ?? [];

        return new PollDto(
            poll.Id, poll.Title, poll.Description, poll.Question, poll.AllowMultiple, poll.IsActive, poll.ArticleId,
            poll.ParticipantCount, total, myOptionIds is not null, poll.CreatedAt,
            poll.Options.Select(o => new PollOptionDto(
                o.Id, o.Text, o.VoteCount,
                total == 0 ? 0 : (int)Math.Round(o.VoteCount * 100.0 / total),
                mine.Contains(o.Id))).ToList());
    }
}
