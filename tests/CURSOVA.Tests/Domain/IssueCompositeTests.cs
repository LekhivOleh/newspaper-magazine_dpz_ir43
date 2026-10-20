using CURSOVA.Domain.Common;
using CURSOVA.Domain.Issues;

namespace CURSOVA.Tests.Domain;

public class IssueCompositeTests
{
    [Fact]
    public void Sections_and_articles_are_treated_uniformly()
    {
        var issue = new Issue("Журнал місяця", "Найкраще за жовтень");
        var reviews = issue.AddSection(issue.Root, "Огляди");
        var phones = issue.AddSection(reviews, "Смартфони");
        issue.AddArticle(issue.Root, 1);
        issue.AddArticle(reviews, 2);
        issue.AddArticle(phones, 3);
        issue.AddArticle(phones, 4);

        Assert.Equal(4, issue.Root.CountArticles());
        Assert.Equal(3, reviews.CountArticles());
        Assert.Equal([1, 2, 3, 4], issue.Root.GetArticleIds().Order());
        Assert.Equal(7, issue.Root.Flatten().Count());
    }

    [Fact]
    public void The_same_article_cannot_appear_twice_anywhere_in_the_tree()
    {
        var issue = new Issue("Випуск", "Опис");
        var section = issue.AddSection(issue.Root, "Розділ");
        issue.AddArticle(issue.Root, 5);

        Assert.Throws<DomainException>(() => issue.AddArticle(section, 5));
    }

    [Fact]
    public void Removing_a_section_removes_its_whole_subtree()
    {
        var issue = new Issue("Випуск", "Опис");
        var section = issue.AddSection(issue.Root, "Розділ").WithId(10);
        issue.AddSection(section, "Підрозділ").WithId(11);
        issue.AddArticle(section, 1).WithId(12);
        issue.AddArticle(issue.Root, 2).WithId(13);

        issue.Remove(10);

        Assert.Equal([2], issue.Root.GetArticleIds());
        Assert.Equal(2, issue.Components.Count);
    }

    [Fact]
    public void An_empty_issue_cannot_be_published()
    {
        var issue = new Issue("Випуск", "Опис");
        issue.AddSection(issue.Root, "Порожній розділ");

        Assert.Throws<DomainException>(issue.Publish);
    }
}
