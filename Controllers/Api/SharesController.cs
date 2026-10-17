using CURSOVA.Application.Sharing;
using CURSOVA.Domain.Sharing;
using Microsoft.AspNetCore.Mvc;

namespace CURSOVA.Controllers.Api;

[ApiController]
[Route("api/share/{type}/{id:int}")]
public class SharesController(ISharingFacade sharing) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyList<ShareLinkDto>> Links(ShareableType type, int id, CancellationToken ct) =>
        sharing.GetLinksAsync(type, id, ct);

    [HttpGet("stats")]
    public Task<ShareStatsDto> Stats(ShareableType type, int id, CancellationToken ct) =>
        sharing.GetStatsAsync(type, id, ct);
}
