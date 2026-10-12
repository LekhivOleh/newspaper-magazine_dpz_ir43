using CURSOVA.Domain.Common;

namespace CURSOVA.Domain.Interactive;

public abstract class InteractiveElement : Entity
{
    public const int TitleMaxLength = 200;

    public string Title { get; private set; } = null!;
    public string? Description { get; private set; }
    public int? ArticleId { get; private set; }
    public bool IsActive { get; private set; } = true;
    public int ParticipantCount { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    protected InteractiveElement() { }

    protected InteractiveElement(string title, string? description, int? articleId)
    {
        UpdateDetails(title, description, articleId);
    }

    public void UpdateDetails(string title, string? description, int? articleId)
    {
        Title = Guard.NotEmpty(title, "Назва", TitleMaxLength);
        Description = Guard.Optional(description, "Опис", 1000);
        ArticleId = articleId is > 0 ? articleId : null;
    }

    public void Open() => IsActive = true;

    public void Close() => IsActive = false;

    /// <summary>
    /// Template Method: the participation algorithm is fixed here,
    /// subclasses customise individual steps.
    /// </summary>
    public Submission Submit(Participant participant, IEnumerable<int> selectedOptionIds)
    {
        EnsureOpen();
        EnsureCanParticipate(participant);

        var selection = selectedOptionIds.Distinct().ToList();
        ValidateSelection(selection);

        var outcome = Evaluate(selection);

        ParticipantCount++;
        return new Submission(this, participant, selection, outcome);
    }

    protected virtual void EnsureOpen()
    {
        if (!IsActive)
            throw new DomainException("Участь уже закрита.");
    }

    protected virtual void EnsureCanParticipate(Participant participant) { }

    protected abstract void ValidateSelection(IReadOnlyList<int> selection);

    protected abstract SubmissionOutcome Evaluate(IReadOnlyList<int> selection);
}
