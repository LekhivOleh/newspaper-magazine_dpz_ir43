using CURSOVA.Domain.Common;

namespace CURSOVA.Domain.Interactive.Quizzes.Scoring;

/// <summary>Knowledge test: score = number of correct answers, outcome = highest level whose threshold (in %) is reached.</summary>
public class CorrectAnswersScoring : IQuizScoringStrategy
{
    public ScoringMode Mode => ScoringMode.CorrectAnswers;

    public void Validate(Quiz quiz)
    {
        foreach (var question in quiz.Questions)
        {
            if (question.Options.Count(o => o.IsCorrect) != 1)
                throw new DomainException($"Питання «{question.Text}» має мати рівно одну правильну відповідь.");
        }

        if (quiz.Outcomes.Any(o => o.Threshold is < 0 or > 100))
            throw new DomainException("Поріг рівня задається у відсотках: від 0 до 100.");
        if (quiz.Outcomes.All(o => o.Threshold != 0))
            throw new DomainException("Потрібен рівень із порогом 0% — для будь-якого результату.");
    }

    public SubmissionOutcome Score(Quiz quiz, IReadOnlyList<QuizOption> chosen)
    {
        var score = chosen.Count(o => o.IsCorrect);
        var max = quiz.Questions.Count;
        var percent = score * 100 / max;

        var outcome = quiz.Outcomes.Where(o => o.Threshold <= percent).MaxBy(o => o.Threshold)!;
        return new SubmissionOutcome(outcome.Key, score, max);
    }
}
