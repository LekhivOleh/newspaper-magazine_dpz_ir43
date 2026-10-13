using CURSOVA.Domain.Common;
using CURSOVA.Domain.Interactive.Quizzes.Scoring;

namespace CURSOVA.Domain.Interactive.Quizzes;

public class Quiz : InteractiveElement
{
    private readonly List<QuizQuestion> _questions = [];
    private readonly List<QuizOutcome> _outcomes = [];

    public ScoringMode ScoringMode { get; private set; }
    public string? CoverImageUrl { get; private set; }

    public IReadOnlyList<QuizQuestion> Questions => _questions.OrderBy(q => q.Position).ToList();
    public IReadOnlyList<QuizOutcome> Outcomes => _outcomes.OrderBy(o => o.Position).ToList();

    private IQuizScoringStrategy Scoring => QuizScoring.For(ScoringMode);

    private Quiz() { }

    public Quiz(string title, ScoringMode scoringMode, string? description = null, string? coverImageUrl = null, int? articleId = null)
        : base(title, description, articleId)
    {
        ScoringMode = scoringMode;
        CoverImageUrl = Guard.Optional(coverImageUrl, "Обкладинка", 500);
    }

    public void UpdateCover(string? coverImageUrl) => CoverImageUrl = Guard.Optional(coverImageUrl, "Обкладинка", 500);

    /// <summary>Replaces questions and outcomes; the active scoring strategy validates the result.</summary>
    public void Define(ScoringMode scoringMode, IReadOnlyList<QuestionSpec> questions, IReadOnlyList<OutcomeSpec> outcomes)
    {
        if (ParticipantCount > 0)
            throw new DomainException("Тест уже проходили — змінювати питання не можна.");
        if (questions.Count == 0)
            throw new DomainException("Тест має містити хоча б одне питання.");
        if (outcomes.Count == 0)
            throw new DomainException("Тест має містити хоча б один результат.");
        if (outcomes.Select(o => o.Key.Trim()).Distinct().Count() != outcomes.Count)
            throw new DomainException("Ключі результатів не повинні повторюватися.");

        ScoringMode = scoringMode;
        _questions.Clear();
        _outcomes.Clear();

        for (var i = 0; i < questions.Count; i++)
            _questions.Add(new QuizQuestion(questions[i], i));
        for (var i = 0; i < outcomes.Count; i++)
            _outcomes.Add(new QuizOutcome(outcomes[i], i));

        Scoring.Validate(this);
    }

    public QuizOutcome? FindOutcome(string? key) => _outcomes.FirstOrDefault(o => o.Key == key);

    protected override void EnsureOpen()
    {
        base.EnsureOpen();
        if (_questions.Count == 0)
            throw new DomainException("Тест ще не має питань.");
    }

    protected override void ValidateSelection(IReadOnlyList<int> selection)
    {
        foreach (var question in _questions)
        {
            var answered = question.Options.Count(o => selection.Contains(o.Id));
            if (answered == 0)
                throw new DomainException($"Дайте відповідь на питання «{question.Text}».");
            if (answered > 1)
                throw new DomainException($"На питання «{question.Text}» можна обрати лише одну відповідь.");
        }

        if (selection.Count != _questions.Count)
            throw new DomainException("Обрано неіснуючий варіант.");
    }

    protected override SubmissionOutcome Evaluate(IReadOnlyList<int> selection)
    {
        var chosen = _questions.SelectMany(q => q.Options).Where(o => selection.Contains(o.Id)).ToList();
        return Scoring.Score(this, chosen);
    }
}
