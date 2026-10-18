using CURSOVA.Domain.Articles.Blocks;

namespace CURSOVA.Domain.Articles.Building;

public class ContentBuilder : IContentBuilder
{
    private readonly List<ContentBlock> _blocks = [];

    public IContentBuilder AddText(string body, string? heading = null) => Add(new TextBlock(body, heading), false);

    public IContentBuilder AddImage(string url, string? caption = null, bool exclusive = false) =>
        Add(new ImageBlock(url, caption), exclusive);

    public IContentBuilder AddVideo(string embedUrl, string? caption = null, bool exclusive = false) =>
        Add(new VideoBlock(embedUrl, caption), exclusive);

    public IContentBuilder AddGallery(Action<GalleryBuilder> configure, string? title = null, bool exclusive = false)
    {
        var gallery = new GalleryBuilder();
        configure(gallery);
        return Add(gallery.Build(title), exclusive);
    }

    public IReadOnlyList<ContentBlock> Build() => _blocks.ToList();

    private ContentBuilder Add(ContentBlock block, bool exclusive)
    {
        block.IsExclusive = exclusive;
        _blocks.Add(block);
        return this;
    }
}
