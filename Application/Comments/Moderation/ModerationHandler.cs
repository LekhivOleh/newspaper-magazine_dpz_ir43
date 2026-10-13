namespace CURSOVA.Application.Comments.Moderation;

public enum ModerationDecision
{
    Approve,
    Pending,
    Reject
}

/// <summary>What a handler sees: the (possibly already modified) text plus context about the author.</summary>
public record CommentDraft(string Text, int RecentCommentCount, IReadOnlyCollection<string> PreviousTexts);

public record ModerationResult(ModerationDecision Decision, string Text, string? Reason = null);

/// <summary>Chain of Responsibility: every handler either decides or passes the draft to the next one.</summary>
public abstract class ModerationHandler
{
    private ModerationHandler? _next;

    public ModerationHandler SetNext(ModerationHandler next)
    {
        _next = next;
        return next;
    }

    public ModerationResult Handle(CommentDraft draft) => Process(draft);

    protected abstract ModerationResult Process(CommentDraft draft);

    protected ModerationResult PassOn(CommentDraft draft) =>
        _next?.Handle(draft) ?? new ModerationResult(ModerationDecision.Approve, draft.Text);

    protected static ModerationResult Reject(CommentDraft draft, string reason) =>
        new(ModerationDecision.Reject, draft.Text, reason);
}
