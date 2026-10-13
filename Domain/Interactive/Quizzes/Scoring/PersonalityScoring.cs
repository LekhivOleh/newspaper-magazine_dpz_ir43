using CURSOVA.Domain.Common;

namespace CURSOVA.Domain.Interactive.Quizzes.Scoring;

/// <summary>"Which X are you?": every answer votes for an outcome, the most frequent one wins (ties go to the earlier outcome).</summary>
public class PersonalityScoring : IQuizScoringStrategy
{
    public ScoringMode Mode => ScoringMode.Personality;

    public void Validate(Quiz quiz)
    {
        if (quiz.Outcomes.Count < 2)
            throw new DomainException("Для тесту-особистості потрібно щонайменше два результати.");

        var keys = quiz.Outcomes.Select(o => o.Key).ToHashSet();
        var unknown = quiz.Questions.SelectMany(q => q.Options).FirstOrDefault(o => o.OutcomeKey is null || !keys.Contains(o.OutcomeKey));
        if (unknown is not null)
            throw new DomainException($"Варіант «{unknown.Text}» має вказувати на один із результатів.");
    }

    public SubmissionOutcome Score(Quiz quiz, IReadOnlyList<QuizOption> chosen)
    {
        var tally = chosen.GroupBy(o => o.OutcomeKey!).ToDictionary(g => g.Key, g => g.Count());
        var winner = quiz.Outcomes
            .OrderByDescending(o => tally.GetValueOrDefault(o.Key))
            .ThenBy(o => o.Position)
            .First();

        return new SubmissionOutcome(winner.Key, tally.GetValueOrDefault(winner.Key), quiz.Questions.Count);
    }
}
