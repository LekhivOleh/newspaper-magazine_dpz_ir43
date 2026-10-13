namespace CURSOVA.Domain.Interactive.Quizzes.Scoring;

public interface IQuizScoringStrategy
{
    ScoringMode Mode { get; }

    /// <summary>Checks that the quiz definition makes sense for this scoring approach.</summary>
    void Validate(Quiz quiz);

    SubmissionOutcome Score(Quiz quiz, IReadOnlyList<QuizOption> chosen);
}

public static class QuizScoring
{
    private static readonly Dictionary<ScoringMode, IQuizScoringStrategy> Strategies =
        new IQuizScoringStrategy[] { new CorrectAnswersScoring(), new PersonalityScoring(), new PointsScoring() }
            .ToDictionary(s => s.Mode);

    public static IQuizScoringStrategy For(ScoringMode mode) => Strategies[mode];
}
