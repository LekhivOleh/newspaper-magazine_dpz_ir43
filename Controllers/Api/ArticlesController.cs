using CURSOVA.Application.Abstractions.Identity;
using CURSOVA.Application.Articles;
using CURSOVA.Application.Articles.Access;
using CURSOVA.Application.Common;
using CURSOVA.Domain.Articles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CURSOVA.Controllers.Api;

[ApiController]
[Route("api/articles")]
public class ArticlesController(IArticleService articles, IArticleReader reader, ICurrentUser currentUser) : ControllerBase
{
    [HttpGet]
    public Task<PagedResult<ArticleSummaryDto>> Search([FromQuery] ArticleFilter filter, CancellationToken ct) =>
        currentUser.IsAdmin
            ? articles.SearchAllAsync(filter, ct)
            : articles.SearchAsync(filter with { Status = ArticleStatus.Published }, ct);

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ArticleDetailsDto>> Get(int id, CancellationToken ct) =>
        VisibleOrNotFound(await reader.GetAsync(id, ct));

    [HttpGet("{slug}")]
    public async Task<ActionResult<ArticleDetailsDto>> GetBySlug(string slug, CancellationToken ct) =>
        VisibleOrNotFound(await reader.GetBySlugAsync(slug, ct));

    [Authorize(Roles = Roles.Admin)]
    [HttpPost]
    [ProducesResponseType<ArticleDetailsDto>(StatusCodes.Status201Created)]
    public async Task<ActionResult<ArticleDetailsDto>> Create(SaveArticleRequest request, CancellationToken ct)
    {
        var created = await articles.CreateAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { id = created.Summary.Id }, created);
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpPut("{id:int}")]
    public Task<ArticleDetailsDto> Update(int id, SaveArticleRequest request, CancellationToken ct) =>
        articles.UpdateAsync(id, request, ct);

    [Authorize(Roles = Roles.Admin)]
    [HttpPut("{id:int}/content")]
    public Task<ArticleDetailsDto> ReplaceContent(int id, SaveContentRequest request, CancellationToken ct) =>
        articles.ReplaceContentAsync(id, request, ct);

    [Authorize(Roles = Roles.Admin)]
    [HttpPost("{id:int}/publish")]
    public Task<ArticleDetailsDto> Publish(int id, CancellationToken ct) => articles.PublishAsync(id, ct);

    [Authorize(Roles = Roles.Admin)]
    [HttpPost("{id:int}/archive")]
    public Task<ArticleDetailsDto> Archive(int id, CancellationToken ct) => articles.ArchiveAsync(id, ct);

    [Authorize(Roles = Roles.Admin)]
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await articles.DeleteAsync(id, ct);
        return NoContent();
    }

    private ActionResult<ArticleDetailsDto> VisibleOrNotFound(ArticleDetailsDto article) =>
        article.Summary.Status == ArticleStatus.Published || currentUser.IsAdmin ? article : NotFound();
}
