using CURSOVA.Application.Common;
using CURSOVA.Application.Sharing;
using CURSOVA.Domain.Common;
using CURSOVA.Domain.Sharing;
using Microsoft.AspNetCore.Mvc;

namespace CURSOVA.Controllers;

public class ShareController(ISharingFacade sharing) : Controller
{
    /// <summary>Counts the share, then sends the reader on to the social network.</summary>
    [HttpGet("/share/{type}/{id:int}/{network}")]
    public async Task<IActionResult> Go(string type, int id, string network, CancellationToken ct)
    {
        if (!Enum.TryParse<ShareableType>(type, ignoreCase: true, out var contentType))
            return NotFound();

        try
        {
            return Redirect(await sharing.ShareAsync(contentType, id, network, ct));
        }
        catch (Exception e) when (e is NotFoundException or DomainException)
        {
            return NotFound();
        }
    }
}
