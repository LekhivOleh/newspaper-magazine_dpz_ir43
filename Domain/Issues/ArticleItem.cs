using CURSOVA.Domain.Common;

namespace CURSOVA.Domain.Issues;

public class ArticleItem : IssueComponent
{
    public int ArticleId { get; private set; }
    public string? EditorNote { get; private set; }

    private ArticleItem() { }

    public ArticleItem(int articleId, string? editorNote = null)
    {
        if (articleId <= 0)
            throw new DomainException("Оберіть статтю.");

        ArticleId = articleId;
        EditorNote = Guard.Optional(editorNote, "Коментар редактора", 300);
    }

    public override int CountArticles() => 1;

    public override IEnumerable<int> GetArticleIds()
    {
        yield return ArticleId;
    }
}
