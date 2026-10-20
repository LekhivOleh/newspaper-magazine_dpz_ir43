using CURSOVA.Application.Articles;
using CURSOVA.Domain.Articles;
using CURSOVA.Domain.Common;

namespace CURSOVA.Tests;

internal static class TestSupport
{
    public static readonly DateTime Now = new(2026, 10, 1, 12, 0, 0, DateTimeKind.Utc);

    /// <summary>Entities get ids from the database; tests that look things up by id assign them by hand.</summary>
    public static T WithId<T>(this T entity, int id) where T : Entity
    {
        typeof(Entity).GetProperty(nameof(Entity.Id))!.SetValue(entity, id);
        return entity;
    }

    public static ArticleDetailsDto Article(
        IReadOnlyList<BlockDto> blocks,
        AccessLevel access = AccessLevel.Free,
        int? sponsorPartnerId = null,
        int categoryId = 1) =>
        new(new ArticleSummaryDto(
                1, "Тестова стаття", "testova-stattia", "Лід", null,
                new CategoryRefDto(categoryId, "Гаджети", "gadzhety"),
                new AuthorDto(1, "Автор", null, null, null),
                ArticleStatus.Published, access, Now, Now, 0, sponsorPartnerId),
            blocks);

    public static TextBlockDto Text(string body = "Абзац") => new(body, null);

    public static ImageBlockDto ExclusiveImage() => new("https://example.com/a.jpg", null) { IsExclusive = true };
}

internal sealed class FixedClock(DateTime utcNow) : TimeProvider
{
    public DateTime UtcNow { get; set; } = utcNow;

    public override DateTimeOffset GetUtcNow() => new(UtcNow);
}
