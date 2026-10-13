using CURSOVA.Domain.Common;

namespace CURSOVA.Domain.Interactive.Quizzes;

public enum ScoringMode
{
    CorrectAnswers,
    Personality,
    Points
}

public record OptionSpec(string Text, bool IsCorrect = false, string? OutcomeKey = null, int Points = 0);

public record QuestionSpec(string Text, IReadOnlyList<OptionSpec> Options);

public record OutcomeSpec(string Key, string Title, string? Description = null, int Threshold = 0);

public class QuizQuestion : Entity
{
    private readonly List<QuizOption> _options = [];

    public int QuizId { get; private set; }
    public string Text { get; private set; } = null!;
    public int Position { get; private set; }

    public IReadOnlyList<QuizOption> Options => _options.OrderBy(o => o.Position).ToList();

    private QuizQuestion() { }

    internal QuizQuestion(QuestionSpec spec, int position)
    {
        Text = Guard.NotEmpty(spec.Text, "Питання", 300);
        Position = position;

        if (spec.Options.Count < 2)
            throw new DomainException($"Питання «{Text}» має містити щонайменше два варіанти.");

        for (var i = 0; i < spec.Options.Count; i++)
            _options.Add(new QuizOption(spec.Options[i], i));
    }
}

public class QuizOption : Entity
{
    public int QuestionId { get; private set; }
    public string Text { get; private set; } = null!;
    public int Position { get; private set; }
    public bool IsCorrect { get; private set; }
    public string? OutcomeKey { get; private set; }
    public int Points { get; private set; }

    private QuizOption() { }

    internal QuizOption(OptionSpec spec, int position)
    {
        Text = Guard.NotEmpty(spec.Text, "Варіант відповіді", 200);
        Position = position;
        IsCorrect = spec.IsCorrect;
        OutcomeKey = Guard.Optional(spec.OutcomeKey, "Ключ результату", 50);
        Points = spec.Points;
    }
}

public class QuizOutcome : Entity
{
    public int QuizId { get; private set; }
    public string Key { get; private set; } = null!;
    public string Title { get; private set; } = null!;
    public string? Description { get; private set; }
    public int Threshold { get; private set; }
    public int Position { get; private set; }

    private QuizOutcome() { }

    internal QuizOutcome(OutcomeSpec spec, int position)
    {
        Key = Guard.NotEmpty(spec.Key, "Ключ результату", 50);
        Title = Guard.NotEmpty(spec.Title, "Назва результату", 150);
        Description = Guard.Optional(spec.Description, "Опис результату", 1000);
        Threshold = spec.Threshold;
        Position = position;
    }
}
