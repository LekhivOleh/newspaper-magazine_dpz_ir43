using CURSOVA.Domain.Articles;
using CURSOVA.Domain.Articles.Blocks;
using CURSOVA.Domain.Articles.Building;
using CURSOVA.Domain.Common;

namespace CURSOVA.Tests.Domain;

public class ArticleBuilderTests
{
    [Fact]
    public void Builds_a_glossy_article_step_by_step()
    {
        var article = ArticleBuilder.Create("Огляд смартфона")
            .WithLead("Коротко про головне.")
            .InCategory(3)
            .By(7)
            .Premium()
            .WithContent(c => c
                .AddText("Вступ", "Дизайн")
                .AddImage("https://example.com/phone.jpg", "Фронтальна панель")
                .AddVideo("https://www.youtube.com/embed/abc", exclusive: true)
                .AddGallery(g => g.Add("https://example.com/1.jpg").Add("https://example.com/2.jpg"), "Фото"))
            .Published()
            .Build();

        Assert.Equal("ohliad-smartfona", article.Slug);
        Assert.True(article.IsPremium);
        Assert.True(article.IsPublished);
        Assert.Equal([typeof(TextBlock), typeof(ImageBlock), typeof(VideoBlock), typeof(GalleryBlock)],
            article.Blocks.Select(b => b.GetType()));
        Assert.Equal([0, 1, 2, 3], article.Blocks.Select(b => b.Position));
        Assert.True(article.Blocks[2].IsExclusive);
        Assert.Equal(2, ((GalleryBlock)article.Blocks[3]).Images.Count);
    }

    [Fact]
    public void Lead_is_required()
    {
        var builder = ArticleBuilder.Create("Без ліду").InCategory(1).By(1);

        Assert.Throws<DomainException>(() => builder.Build());
    }

    [Fact]
    public void Cannot_publish_without_content()
    {
        var builder = ArticleBuilder.Create("Порожня").WithLead("Лід").InCategory(1).By(1).Published();

        Assert.Throws<DomainException>(() => builder.Build());
    }

    [Fact]
    public void Content_builder_can_be_used_on_its_own()
    {
        var content = new ContentBuilder();
        content.AddText("Один").AddText("Два");

        var blocks = content.Build();

        Assert.Equal(2, blocks.Count);
        Assert.All(blocks, b => Assert.Equal(BlockType.Text, b.Type));
    }
}
