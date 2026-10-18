using CURSOVA.Application.Profiles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CURSOVA.Controllers.Api;

[ApiController]
public class CollectionsController(ICollectionService collections) : ControllerBase
{
    [HttpGet("api/collections")]
    public Task<IReadOnlyList<CollectionSummaryDto>> Public(CancellationToken ct) => collections.ListPublicAsync(ct);

    [HttpGet("api/collections/{id:int}")]
    public Task<CollectionDto> Get(int id, CancellationToken ct) => collections.GetAsync(id, ct);

    [Authorize]
    [HttpPost("api/collections/{id:int}/copy")]
    public async Task<IActionResult> Copy(int id, CancellationToken ct)
    {
        var copyId = await collections.CopyAsync(id, ct);
        return CreatedAtAction(nameof(Get), new { id = copyId }, await collections.GetAsync(copyId, ct));
    }

    [Authorize]
    [HttpGet("api/me/collections")]
    public Task<IReadOnlyList<CollectionSummaryDto>> Mine(CancellationToken ct) => collections.ListMineAsync(ct);

    [Authorize]
    [HttpPost("api/me/collections")]
    public async Task<IActionResult> Create(SaveCollectionRequest request, CancellationToken ct)
    {
        var created = await collections.CreateAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [Authorize]
    [HttpPut("api/me/collections/{id:int}")]
    public async Task<IActionResult> Update(int id, SaveCollectionRequest request, CancellationToken ct)
    {
        await collections.UpdateAsync(id, request, ct);
        return NoContent();
    }

    [Authorize]
    [HttpDelete("api/me/collections/{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await collections.DeleteAsync(id, ct);
        return NoContent();
    }

    [Authorize]
    [HttpPost("api/me/collections/{id:int}/items")]
    public async Task<IActionResult> AddItem(int id, AddToCollectionRequest request, CancellationToken ct)
    {
        await collections.AddArticleAsync(id, request.ArticleId, request.Note, ct);
        return NoContent();
    }

    [Authorize]
    [HttpDelete("api/me/collections/{id:int}/items/{articleId:int}")]
    public async Task<IActionResult> RemoveItem(int id, int articleId, CancellationToken ct)
    {
        await collections.RemoveArticleAsync(id, articleId, ct);
        return NoContent();
    }
}
