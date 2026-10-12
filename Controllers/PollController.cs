using CURSOVA.Application.Common;
using CURSOVA.Application.Interactive;
using CURSOVA.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace CURSOVA.Controllers;

[Route("polls")]
public class PollController(IPollService polls) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken ct) => View(await polls.ListAsync(activeOnly: false, ct));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Show(int id, CancellationToken ct)
    {
        try
        {
            return View(await polls.GetAsync(id, ct));
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("{id:int}/vote")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Vote(int id, int[] optionIds, string? returnUrl, CancellationToken ct)
    {
        try
        {
            await polls.VoteAsync(id, optionIds, ct);
        }
        catch (DomainException e)
        {
            TempData[$"PollError:{id}"] = e.Message;
        }
        catch (NotFoundException)
        {
            return NotFound();
        }

        var target = Url.IsLocalUrl(returnUrl) ? returnUrl : Url.Action(nameof(Show), new { id })!;
        return LocalRedirect($"{target}#poll-{id}");
    }
}
