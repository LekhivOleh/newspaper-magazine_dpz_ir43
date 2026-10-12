using CURSOVA.Application.Common;
using CURSOVA.Application.Issues;
using Microsoft.AspNetCore.Mvc;

namespace CURSOVA.Controllers;

[Route("issues")]
public class IssueController(IIssueService issues) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken ct) => View(await issues.ListAsync(publishedOnly: true, ct));

    [HttpGet("{slug}")]
    public async Task<IActionResult> Show(string slug, CancellationToken ct)
    {
        try
        {
            return View(await issues.GetPublishedBySlugAsync(slug, ct));
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
    }
}
