using CURSOVA.Application.Abstractions.Identity;
using CURSOVA.Application.Common;
using CURSOVA.Application.Profiles;
using CURSOVA.Domain.Common;
using CURSOVA.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CURSOVA.Controllers;

[Authorize]
[AutoValidateAntiforgeryToken]
public class ProfileController(ISavedArticleService saved, ICollectionService collections, ICurrentUser currentUser) : Controller
{
    [HttpGet("/profile")]
    public IActionResult Index() => RedirectToAction(nameof(Saved));

    [HttpGet("/profile/saved")]
    public async Task<IActionResult> Saved(int page = 1, CancellationToken ct = default)
    {
        var result = await saved.ListMineAsync(page, ct);
        var pager = new PagerViewModel(result.Page, result.TotalPages, p => Url.Action(nameof(Saved), new { page = p })!);
        return View(new SavedPageViewModel(currentUser.DisplayName ?? "Читач", result, pager));
    }

    [HttpGet("/profile/collections")]
    public async Task<IActionResult> Collections(CancellationToken ct) =>
        View((currentUser.DisplayName ?? "Читач", await collections.ListMineAsync(ct)));

    [HttpPost("/article/{articleId:int}/save")]
    public async Task<IActionResult> Toggle(int articleId, string? returnUrl, CancellationToken ct)
    {
        try
        {
            TempData["SaveMessage"] = await saved.ToggleAsync(articleId, ct)
                ? "Статтю збережено в профілі."
                : "Статтю прибрано зі збережених.";
        }
        catch (Exception e) when (e is DomainException or NotFoundException)
        {
            TempData["SaveMessage"] = e.Message;
        }

        return LocalRedirect(Url.IsLocalUrl(returnUrl) ? returnUrl : Url.Action(nameof(Saved))!);
    }
}
