using CURSOVA.Domain.Common;
using CURSOVA.Domain.Interactive;
using CURSOVA.Domain.Interactive.Polls;

namespace CURSOVA.Tests.Domain;

public class InteractiveTemplateMethodTests
{
    private static readonly Participant Guest = new("anon:1", null, HasParticipated: false);

    private static Poll NewPoll(bool allowMultiple = false)
    {
        var poll = new Poll("Опитування", "Яка ОС вам ближча?", ["Android", "iOS", "Інше"], allowMultiple);
        for (var i = 0; i < poll.Options.Count; i++)
            poll.Options[i].WithId(i + 1);
        return poll;
    }

    [Fact]
    public void Poll_vote_runs_every_step_and_counts_the_participant()
    {
        var poll = NewPoll();

        var submission = poll.Submit(Guest, [2]);

        Assert.Equal([2], submission.SelectedOptionIds);
        Assert.Equal(1, poll.Options.Single(o => o.Id == 2).VoteCount);
        Assert.Equal(1, poll.ParticipantCount);
    }

    [Fact]
    public void Poll_hook_rejects_a_second_vote()
    {
        var poll = NewPoll();

        Assert.Throws<DomainException>(() => poll.Submit(Guest with { HasParticipated = true }, [1]));
        Assert.Equal(0, poll.ParticipantCount);
    }

    [Fact]
    public void Closed_element_rejects_submissions()
    {
        var poll = NewPoll();
        poll.Close();

        Assert.Throws<DomainException>(() => poll.Submit(Guest, [1]));
    }

    [Theory]
    [InlineData(false, new[] { 1, 2 })]
    [InlineData(false, new int[0])]
    [InlineData(true, new[] { 99 })]
    public void Poll_validates_the_selection(bool allowMultiple, int[] selection)
    {
        var poll = NewPoll(allowMultiple);

        Assert.Throws<DomainException>(() => poll.Submit(Guest, selection));
        Assert.Equal(0, poll.TotalVotes);
    }

    [Fact]
    public void Duplicate_ids_in_a_selection_count_once()
    {
        var poll = NewPoll(allowMultiple: true);

        poll.Submit(Guest, [1, 1, 3]);

        Assert.Equal(2, poll.TotalVotes);
    }

    [Fact]
    public void Quiz_shares_the_algorithm_but_requires_every_question_answered()
    {
        var quiz = QuizScoringTests.KnowledgeQuiz();

        Assert.Throws<DomainException>(() => quiz.Submit(Guest, [QuizScoringTests.Option(quiz, 0, 0).Id]));
        Assert.Equal(0, quiz.ParticipantCount);
    }
}
