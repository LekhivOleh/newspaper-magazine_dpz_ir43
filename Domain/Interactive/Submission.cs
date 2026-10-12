using CURSOVA.Domain.Common;

namespace CURSOVA.Domain.Interactive;

public class Submission : Entity
{
    public int ElementId { get; private set; }
    public string ParticipantKey { get; private set; } = null!;
    public int? UserId { get; private set; }
    public List<int> SelectedOptionIds { get; private set; } = [];
    public string? ResultKey { get; private set; }
    public int? Score { get; private set; }
    public int? MaxScore { get; private set; }
    public DateTime SubmittedAt { get; private set; }

    private Submission() { }

    internal Submission(InteractiveElement element, Participant participant, IReadOnlyList<int> selection, SubmissionOutcome outcome)
    {
        ElementId = element.Id;
        ParticipantKey = participant.Key;
        UserId = participant.UserId;
        SelectedOptionIds = selection.ToList();
        ResultKey = outcome.ResultKey;
        Score = outcome.Score;
        MaxScore = outcome.MaxScore;
        SubmittedAt = DateTime.UtcNow;
    }
}
