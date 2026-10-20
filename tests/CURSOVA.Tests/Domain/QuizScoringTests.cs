using CURSOVA.Domain.Common;
using CURSOVA.Domain.Interactive;
using CURSOVA.Domain.Interactive.Quizzes;
using CURSOVA.Domain.Interactive.Quizzes.Scoring;

namespace CURSOVA.Tests.Domain;

public class QuizScoringTests
{
    private static readonly Participant Guest = new("anon:1", null, false);

    internal static QuizOption Option(Quiz quiz, int question, int option) => quiz.Questions[question].Options[option];

    private static Quiz WithIds(Quiz quiz)
    {
        var id = 1;
        foreach (var option in quiz.Questions.SelectMany(q => q.Options))
            option.WithId(id++);
        return quiz;
    }

    internal static Quiz KnowledgeQuiz()
    {
        var quiz = new Quiz("Знавець гаджетів", ScoringMode.CorrectAnswers);
        quiz.Define(ScoringMode.CorrectAnswers,
            [
                new QuestionSpec("Що таке OLED?", [new OptionSpec("Тип дисплея", IsCorrect: true), new OptionSpec("Процесор")]),
                new QuestionSpec("Що таке SSD?", [new OptionSpec("Накопичувач", IsCorrect: true), new OptionSpec("Відеокарта")])
            ],
            [new OutcomeSpec("novice", "Новачок", Threshold: 0), new OutcomeSpec("pro", "Профі", Threshold: 100)]);
        return WithIds(quiz);
    }

    private static Quiz PersonalityQuiz()
    {
        var quiz = new Quiz("Який ви гаджет?", ScoringMode.Personality);
        quiz.Define(ScoringMode.Personality,
            [
                new QuestionSpec("Вихідні?", [new OptionSpec("Гори", OutcomeKey: "watch"), new OptionSpec("Ігри", OutcomeKey: "console")]),
                new QuestionSpec("Ранок?", [new OptionSpec("Пробіжка", OutcomeKey: "watch"), new OptionSpec("Стрім", OutcomeKey: "console")]),
                new QuestionSpec("Музика?", [new OptionSpec("Подкаст", OutcomeKey: "watch"), new OptionSpec("Саундтрек", OutcomeKey: "console")])
            ],
            [new OutcomeSpec("watch", "Смарт-годинник"), new OutcomeSpec("console", "Консоль")]);
        return WithIds(quiz);
    }

    private static Quiz PointsQuiz()
    {
        var quiz = new Quiz("Наскільки ви залежні від смартфона?", ScoringMode.Points);
        quiz.Define(ScoringMode.Points,
            [
                new QuestionSpec("Скільки годин на день?", [new OptionSpec("1", Points: 0), new OptionSpec("5+", Points: 3)]),
                new QuestionSpec("Телефон у спальні?", [new OptionSpec("Ні", Points: 0), new OptionSpec("Так", Points: 2)])
            ],
            [new OutcomeSpec("calm", "Спокійно", Threshold: 0), new OutcomeSpec("hooked", "Залежність", Threshold: 4)]);
        return WithIds(quiz);
    }

    [Fact]
    public void Correct_answers_strategy_scores_right_answers_and_picks_a_level()
    {
        var quiz = KnowledgeQuiz();

        var half = quiz.Submit(Guest, [Option(quiz, 0, 0).Id, Option(quiz, 1, 1).Id]);
        var all = quiz.Submit(Guest, [Option(quiz, 0, 0).Id, Option(quiz, 1, 0).Id]);

        Assert.Equal(("novice", (int?)1, (int?)2), (half.ResultKey, half.Score, half.MaxScore));
        Assert.Equal(("pro", (int?)2, (int?)2), (all.ResultKey, all.Score, all.MaxScore));
    }

    [Fact]
    public void Personality_strategy_picks_the_most_frequent_outcome()
    {
        var quiz = PersonalityQuiz();

        var result = quiz.Submit(Guest, [Option(quiz, 0, 1).Id, Option(quiz, 1, 0).Id, Option(quiz, 2, 1).Id]);

        Assert.Equal("console", result.ResultKey);
        Assert.Equal(2, result.Score);
    }

    [Fact]
    public void Points_strategy_maps_the_total_to_the_highest_reached_band()
    {
        var quiz = PointsQuiz();

        var result = quiz.Submit(Guest, [Option(quiz, 0, 1).Id, Option(quiz, 1, 1).Id]);

        Assert.Equal(("hooked", (int?)5, (int?)5), (result.ResultKey, result.Score, result.MaxScore));
    }

    [Fact]
    public void Each_strategy_validates_its_own_kind_of_quiz()
    {
        var quiz = new Quiz("Тест", ScoringMode.CorrectAnswers);

        Assert.Throws<DomainException>(() => quiz.Define(ScoringMode.CorrectAnswers,
            [new QuestionSpec("Дві правильні?", [new OptionSpec("А", true), new OptionSpec("Б", true)])],
            [new OutcomeSpec("any", "Будь-хто")]));
        Assert.Throws<DomainException>(() => quiz.Define(ScoringMode.Personality,
            [new QuestionSpec("Без результату?", [new OptionSpec("А", OutcomeKey: "missing"), new OptionSpec("Б", OutcomeKey: "a")])],
            [new OutcomeSpec("a", "А"), new OutcomeSpec("b", "Б")]));
    }

    [Theory]
    [InlineData(ScoringMode.CorrectAnswers, typeof(CorrectAnswersScoring))]
    [InlineData(ScoringMode.Personality, typeof(PersonalityScoring))]
    [InlineData(ScoringMode.Points, typeof(PointsScoring))]
    public void Strategy_is_chosen_by_scoring_mode(ScoringMode mode, Type expected) =>
        Assert.IsType(expected, QuizScoring.For(mode));
}
