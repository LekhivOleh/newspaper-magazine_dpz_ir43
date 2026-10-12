using CURSOVA.Domain.Common;

namespace CURSOVA.Domain.Issues;

public abstract class IssueComponent : Entity
{
    public int IssueId { get; private set; }
    public int? ParentId { get; private set; }
    public IssueSection? Parent { get; private set; }
    public int Position { get; internal set; }

    public abstract int CountArticles();

    public abstract IEnumerable<int> GetArticleIds();

    public virtual IEnumerable<IssueComponent> Flatten()
    {
        yield return this;
    }

    internal void AttachTo(IssueSection parent, int position)
    {
        Parent = parent;
        Position = position;
    }
}
