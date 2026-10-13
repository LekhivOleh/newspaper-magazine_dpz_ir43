namespace CURSOVA.Application.Comments.Moderation;

public interface ICommentModerator
{
    ModerationResult Moderate(CommentDraft draft);
}

/// <summary>Assembles the chain in one place; reordering or adding a rule happens only here.</summary>
public class ModerationPipeline : ICommentModerator
{
    private readonly ModerationHandler _head;

    public ModerationPipeline()
    {
        _head = new LengthHandler();
        _head.SetNext(new RateLimitHandler())
            .SetNext(new DuplicateHandler())
            .SetNext(new ProfanityHandler())
            .SetNext(new LinkHandler());
    }

    public ModerationResult Moderate(CommentDraft draft) => _head.Handle(draft);
}
