using CURSOVA.Domain.Common;

namespace CURSOVA.Domain.Articles.Building;

public class ArticleBuilder : IContentBuilder
{
    private readonly string _title;
    private readonly ContentBuilder _content = new();
    private string? _lead;
    private string? _coverImageUrl;
    private int _categoryId;
    private int _authorId;
    private AccessLevel _accessLevel = AccessLevel.Free;
    private bool _publish;

    private ArticleBuilder(string title) => _title = title;

    public static ArticleBuilder Create(string title) => new(title);

    public ArticleBuilder WithLead(string lead)
    {
        _lead = lead;
        return this;
    }

    public ArticleBuilder WithCover(string? imageUrl)
    {
        _coverImageUrl = imageUrl;
        return this;
    }

    public ArticleBuilder InCategory(int categoryId)
    {
        _categoryId = categoryId;
        return this;
    }

    public ArticleBuilder By(int authorId)
    {
        _authorId = authorId;
        return this;
    }

    public ArticleBuilder WithAccess(AccessLevel accessLevel)
    {
        _accessLevel = accessLevel;
        return this;
    }

    public ArticleBuilder Premium() => WithAccess(AccessLevel.Premium);

    public ArticleBuilder Published()
    {
        _publish = true;
        return this;
    }

    public ArticleBuilder WithContent(Action<IContentBuilder> configure)
    {
        configure(_content);
        return this;
    }

    IContentBuilder IContentBuilder.AddText(string body, string? heading) => WithContent(c => c.AddText(body, heading));
    IContentBuilder IContentBuilder.AddImage(string url, string? caption) => WithContent(c => c.AddImage(url, caption));
    IContentBuilder IContentBuilder.AddVideo(string embedUrl, string? caption) => WithContent(c => c.AddVideo(embedUrl, caption));
    IContentBuilder IContentBuilder.AddGallery(Action<GalleryBuilder> configure, string? title) =>
        WithContent(c => c.AddGallery(configure, title));

    public Article Build()
    {
        if (_lead is null)
            throw new DomainException("Не задано лід статті.");

        var article = new Article(_title, _lead, _categoryId, _authorId, _coverImageUrl);
        article.SetAccessLevel(_accessLevel);
        article.ReplaceContent(_content.Build());

        if (_publish)
            article.Publish();

        return article;
    }
}
