using CURSOVA.Domain.Common;
using CURSOVA.Domain.Profiles;

namespace CURSOVA.Tests.Domain;

public class CollectionPrototypeTests
{
    private static Collection Original()
    {
        var collection = new Collection(userId: 1, "Олена", "Мої улюблені огляди", "Найкраще за рік", isPublic: true).WithId(42);
        collection.AddArticle(10, "Must read");
        collection.AddArticle(20);
        collection.AddArticle(30, "Про ноутбуки");
        collection.Move(30, -1);
        return collection;
    }

    [Fact]
    public void Clone_copies_items_order_and_notes_for_the_new_owner()
    {
        var original = Original();

        var copy = original.CloneFor(userId: 2, "Андрій");

        Assert.Equal(2, copy.UserId);
        Assert.Equal("Андрій", copy.OwnerName);
        Assert.Equal(original.Name, copy.Name);
        Assert.Equal(42, copy.CopiedFromId);
        Assert.False(copy.IsPublic);
        Assert.Equal([10, 30, 20], copy.Items.Select(i => i.ArticleId));
        Assert.Equal(original.Items.Select(i => i.Note), copy.Items.Select(i => i.Note));
    }

    [Fact]
    public void Clone_is_deep_so_changes_do_not_leak_back()
    {
        var original = Original();
        var copy = original.CloneFor(2, "Андрій");

        copy.RemoveArticle(10);
        copy.Move(20, -1);
        copy.AddArticle(99);

        Assert.Equal([10, 30, 20], original.Items.Select(i => i.ArticleId));
        Assert.All(copy.Items, item => Assert.DoesNotContain(item, original.Items));
    }

    [Fact]
    public void Owner_cannot_copy_their_own_collection()
    {
        Assert.Throws<DomainException>(() => Original().CloneFor(1, "Олена"));
    }
}
