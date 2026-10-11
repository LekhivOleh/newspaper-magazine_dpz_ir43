using CURSOVA.Application.Articles;
using CURSOVA.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace CURSOVA.Controllers.Api;

[ApiController]
[Route("api/articles")]
public class ArticlesController(IArticleService articles) : ControllerBase
{
    [HttpGet]
    public Task<PagedResult<ArticleSummaryDto>> Search([FromQuery] ArticleFilter filter, CancellationToken ct) =>
        articles.SearchAsync(filter, ct);

    [HttpGet("{id:int}")]
    public Task<ArticleDetailsDto> Get(int id, CancellationToken ct) => articles.GetAsync(id, ct);

    [HttpGet("{slug}")]
    public Task<ArticleDetailsDto> GetBySlug(string slug, CancellationToken ct) => articles.GetBySlugAsync(slug, ct);

    [HttpPost]
    [ProducesResponseType<ArticleDetailsDto>(StatusCodes.Status201Created)]
    public async Task<ActionResult<ArticleDetailsDto>> Create(SaveArticleRequest request, CancellationToken ct)
    {
        var created = await articles.CreateAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { id = created.Summary.Id }, created);
    }

    [HttpPut("{id:int}")]
    public Task<ArticleDetailsDto> Update(int id, SaveArticleRequest request, CancellationToken ct) =>
        articles.UpdateAsync(id, request, ct);

    [HttpPost("{id:int}/publish")]
    public Task<ArticleDetailsDto> Publish(int id, CancellationToken ct) => articles.PublishAsync(id, ct);

    [HttpPost("{id:int}/archive")]
    public Task<ArticleDetailsDto> Archive(int id, CancellationToken ct) => articles.ArchiveAsync(id, ct);

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await articles.DeleteAsync(id, ct);
        return NoContent();
    }
}
