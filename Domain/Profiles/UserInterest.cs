using CURSOVA.Domain.Common;

namespace CURSOVA.Domain.Profiles;

/// <summary>A category the reader explicitly marked as interesting.</summary>
public class UserInterest : Entity
{
    public int UserId { get; private set; }
    public int CategoryId { get; private set; }

    private UserInterest() { }

    public UserInterest(int userId, int categoryId)
    {
        UserId = userId;
        CategoryId = categoryId;
    }
}
