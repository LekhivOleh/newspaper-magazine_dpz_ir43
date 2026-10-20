using CURSOVA.Application.Comments.Moderation;

namespace CURSOVA.Tests.Application;

public class ModerationChainTests
{
    private readonly ModerationPipeline _pipeline = new();

    private static CommentDraft Draft(string text, int recent = 0, params string[] previous) => new(text, recent, previous);

    [Fact]
    public void Clean_comment_passes_every_handler_and_is_approved()
    {
        var result = _pipeline.Moderate(Draft("  Чудовий огляд, дякую!  "));

        Assert.Equal(ModerationDecision.Approve, result.Decision);
        Assert.Equal("Чудовий огляд, дякую!", result.Text);
    }

    [Fact]
    public void Too_short_comment_is_stopped_by_the_first_handler()
    {
        var result = _pipeline.Moderate(Draft("ok"));

        Assert.Equal(ModerationDecision.Reject, result.Decision);
        Assert.Contains("короткий", result.Reason);
    }

    [Fact]
    public void Rate_limit_rejects_a_flood()
    {
        var result = _pipeline.Moderate(Draft("Ще один коментар", recent: RateLimitHandler.MaxPerWindow));

        Assert.Equal(ModerationDecision.Reject, result.Decision);
    }

    [Fact]
    public void Duplicate_is_detected_ignoring_case_and_extra_spaces()
    {
        var result = _pipeline.Moderate(Draft("Класна  стаття", 1, "Інший текст", "класна стаття"));

        Assert.Equal(ModerationDecision.Reject, result.Decision);
    }

    [Fact]
    public void Profanity_is_masked_and_the_chain_continues()
    {
        var result = _pipeline.Moderate(Draft("Автор ідіот, але фото гарні"));

        Assert.Equal(ModerationDecision.Approve, result.Decision);
        Assert.Equal("Автор і****, але фото гарні", result.Text);
    }

    [Fact]
    public void Link_sends_comment_to_a_moderator_and_keeps_masking_from_earlier_handlers()
    {
        var result = _pipeline.Moderate(Draft("Дурня, дивіться https://example.com"));

        Assert.Equal(ModerationDecision.Pending, result.Decision);
        Assert.StartsWith("Д****", result.Text);
    }

    [Fact]
    public void Too_many_links_is_spam()
    {
        var result = _pipeline.Moderate(Draft("www.a.com www.b.com www.c.com"));

        Assert.Equal(ModerationDecision.Reject, result.Decision);
    }

    [Fact]
    public void Handlers_can_be_chained_in_any_order()
    {
        var head = new LinkHandler();
        head.SetNext(new LengthHandler());

        Assert.Equal(ModerationDecision.Pending, head.Handle(Draft("x http://a.b")).Decision);
        Assert.Equal(ModerationDecision.Reject, head.Handle(Draft("x")).Decision);
    }
}
