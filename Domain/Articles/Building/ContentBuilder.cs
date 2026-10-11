using CURSOVA.Domain.Articles.Blocks;

namespace CURSOVA.Domain.Articles.Building;

public class ContentBuilder : IContentBuilder
{
    private readonly List<ContentBlock> _blocks = [];

    public IContentBuilder AddText(string body, string? heading = null) => Add(new TextBlock(body, heading));

    public IContentBuilder AddImage(string url, string? caption = null) => Add(new ImageBlock(url, caption));

    public IContentBuilder AddVideo(string embedUrl, string? caption = null) => Add(new VideoBlock(embedUrl, caption));

    public IContentBuilder AddGallery(Action<GalleryBuilder> configure, string? title = null)
    {
        var gallery = new GalleryBuilder();
        configure(gallery);
        return Add(gallery.Build(title));
    }

    public IReadOnlyList<ContentBlock> Build() => _blocks.ToList();

    private ContentBuilder Add(ContentBlock block)
    {
        _blocks.Add(block);
        return this;
    }
}
