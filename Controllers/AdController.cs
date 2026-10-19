using CURSOVA.Application.Advertising;
using CURSOVA.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace CURSOVA.Controllers;

public class AdController(IAdvertisingService ads) : Controller
{
    /// <summary>Counts the click, then sends the reader to the advertiser (or a page on our own site).</summary>
    [HttpGet("/ads/{id:int}/click")]
    public async Task<IActionResult> Click(int id, CancellationToken ct)
    {
        try
        {
            return Redirect(await ads.ClickAsync(id, ct));
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
    }
}
