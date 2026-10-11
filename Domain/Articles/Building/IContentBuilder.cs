namespace CURSOVA.Domain.Articles.Building;

public interface IContentBuilder
{
    IContentBuilder AddText(string body, string? heading = null);
    IContentBuilder AddImage(string url, string? caption = null);
    IContentBuilder AddVideo(string embedUrl, string? caption = null);
    IContentBuilder AddGallery(Action<GalleryBuilder> configure, string? title = null);
}
