using CURSOVA.Domain.Common;

namespace CURSOVA.Domain.Issues;

public class IssueSection : IssueComponent
{
    public const int TitleMaxLength = 150;

    private readonly List<IssueComponent> _children = [];

    public string Title { get; private set; } = null!;

    public IReadOnlyList<IssueComponent> Children => _children.OrderBy(c => c.Position).ToList();

    private IssueSection() { }

    public IssueSection(string title) => Rename(title);

    public void Rename(string title) => Title = Guard.NotEmpty(title, "Назва розділу", TitleMaxLength);

    public override int CountArticles() => _children.Sum(c => c.CountArticles());

    public override IEnumerable<int> GetArticleIds() => Children.SelectMany(c => c.GetArticleIds());

    public override IEnumerable<IssueComponent> Flatten() => base.Flatten().Concat(_children.SelectMany(c => c.Flatten()));

    internal void Add(IssueComponent child)
    {
        var position = _children.Count == 0 ? 0 : _children.Max(c => c.Position) + 1;
        child.AttachTo(this, position);
        _children.Add(child);
    }

    internal void Remove(IssueComponent child)
    {
        _children.Remove(child);
        var position = 0;
        foreach (var c in _children.OrderBy(c => c.Position))
            c.Position = position++;
    }

    internal void Move(IssueComponent child, int delta)
    {
        var ordered = Children.ToList();
        var index = ordered.IndexOf(child);
        var target = index + delta;
        if (index < 0 || target < 0 || target >= ordered.Count)
            return;

        (child.Position, ordered[target].Position) = (ordered[target].Position, child.Position);
    }
}
