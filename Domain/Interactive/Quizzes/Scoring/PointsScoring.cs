using CURSOVA.Domain.Common;

namespace CURSOVA.Domain.Interactive.Quizzes.Scoring;

/// <summary>Self-assessment: answers carry points, the total is mapped to the highest band whose threshold is reached.</summary>
public class PointsScoring : IQuizScoringStrategy
{
    public ScoringMode Mode => ScoringMode.Points;

    public void Validate(Quiz quiz)
    {
        if (quiz.Questions.SelectMany(q => q.Options).Any(o => o.Points < 0))
            throw new DomainException("Бали за відповідь не можуть бути від'ємними.");

        var minimum = quiz.Questions.Sum(q => q.Options.Min(o => o.Points));
        if (quiz.Outcomes.All(o => o.Threshold > minimum))
            throw new DomainException($"Потрібен результат із порогом не більше {minimum} балів — мінімально можливої суми.");
    }

    public SubmissionOutcome Score(Quiz quiz, IReadOnlyList<QuizOption> chosen)
    {
        var score = chosen.Sum(o => o.Points);
        var max = quiz.Questions.Sum(q => q.Options.Max(o => o.Points));

        var outcome = quiz.Outcomes.Where(o => o.Threshold <= score).MaxBy(o => o.Threshold)!;
        return new SubmissionOutcome(outcome.Key, score, max);
    }
}
