using CURSOVA.Domain.Common;

namespace CURSOVA.Domain.Interactive.Polls;

public class Poll : InteractiveElement
{
    public const int QuestionMaxLength = 300;

    private readonly List<PollOption> _options = [];

    public string Question { get; private set; } = null!;
    public bool AllowMultiple { get; private set; }

    public IReadOnlyList<PollOption> Options => _options.OrderBy(o => o.Position).ToList();

    public int TotalVotes => _options.Sum(o => o.VoteCount);

    private Poll() { }

    public Poll(string title, string question, IEnumerable<string> options, bool allowMultiple = false,
        string? description = null, int? articleId = null)
        : base(title, description, articleId)
    {
        Question = Guard.NotEmpty(question, "Питання", QuestionMaxLength);
        AllowMultiple = allowMultiple;
        ReplaceOptions(options);
    }

    public void UpdateQuestion(string question, IEnumerable<string> options, bool allowMultiple)
    {
        var texts = options.Select(o => o.Trim()).Where(o => o.Length > 0).ToList();
        var changed = !texts.SequenceEqual(Options.Select(o => o.Text)) || allowMultiple != AllowMultiple;

        if (changed && ParticipantCount > 0)
            throw new DomainException("Варіанти відповідей не можна змінювати після початку голосування.");

        Question = Guard.NotEmpty(question, "Питання", QuestionMaxLength);
        if (changed)
        {
            AllowMultiple = allowMultiple;
            ReplaceOptions(texts);
        }
    }

    protected override void EnsureCanParticipate(Participant participant)
    {
        if (participant.HasParticipated)
            throw new DomainException("Ви вже проголосували в цьому опитуванні.");
    }

    protected override void ValidateSelection(IReadOnlyList<int> selection)
    {
        if (selection.Count == 0)
            throw new DomainException("Оберіть варіант відповіді.");
        if (!AllowMultiple && selection.Count > 1)
            throw new DomainException("У цьому опитуванні можна обрати лише один варіант.");
        if (selection.Any(id => _options.All(o => o.Id != id)))
            throw new DomainException("Обрано неіснуючий варіант.");
    }

    protected override SubmissionOutcome Evaluate(IReadOnlyList<int> selection)
    {
        foreach (var option in _options.Where(o => selection.Contains(o.Id)))
            option.RegisterVote();

        return SubmissionOutcome.None;
    }

    private void ReplaceOptions(IEnumerable<string> options)
    {
        var texts = options.Select(o => o.Trim()).Where(o => o.Length > 0).ToList();
        if (texts.Count < 2)
            throw new DomainException("Опитування має містити щонайменше два варіанти.");
        if (texts.Count > 10)
            throw new DomainException("Опитування може містити не більше 10 варіантів.");
        if (texts.Distinct(StringComparer.OrdinalIgnoreCase).Count() != texts.Count)
            throw new DomainException("Варіанти відповідей не повинні повторюватися.");

        _options.Clear();
        for (var i = 0; i < texts.Count; i++)
            _options.Add(new PollOption(texts[i], i));
    }
}
