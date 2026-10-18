using CURSOVA.Application.Common;
using CURSOVA.Application.Profiles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CURSOVA.Controllers.Api;

[ApiController]
[Authorize]
[Route("api/me")]
public class MeController(ISavedArticleService saved) : ControllerBase
{
    [HttpGet("saved")]
    public Task<PagedResult<SavedArticleDto>> Saved(int page = 1, CancellationToken ct = default) => saved.ListMineAsync(page, ct);

    [HttpPut("saved/{articleId:int}")]
    public async Task<IActionResult> Save(int articleId, CancellationToken ct)
    {
        await saved.SaveAsync(articleId, ct);
        return NoContent();
    }

    [HttpDelete("saved/{articleId:int}")]
    public async Task<IActionResult> Remove(int articleId, CancellationToken ct)
    {
        await saved.RemoveAsync(articleId, ct);
        return NoContent();
    }
}
