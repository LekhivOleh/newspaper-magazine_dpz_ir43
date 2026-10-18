using CURSOVA.Domain.Common;

namespace CURSOVA.Domain.Profiles;

public class SavedArticle : Entity
{
    public int UserId { get; private set; }
    public int ArticleId { get; private set; }
    public DateTime SavedAt { get; private set; }

    private SavedArticle() { }

    public SavedArticle(int userId, int articleId)
    {
        UserId = userId;
        ArticleId = articleId;
        SavedAt = DateTime.UtcNow;
    }
}
