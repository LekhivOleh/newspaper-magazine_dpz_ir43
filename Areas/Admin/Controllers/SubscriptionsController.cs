using CURSOVA.Application.Subscriptions;
using CURSOVA.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CURSOVA.Areas.Admin.Controllers;

public class SubscriptionsController(ISubscriptionService subscriptions, UserManager<ApplicationUser> users) : AdminController
{
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        ViewData["Stats"] = await subscriptions.GetStatsAsync(ct);
        return View(await subscriptions.ListSubscribersAsync(ct));
    }

    [HttpPost]
    public async Task<IActionResult> Gift(string email, int days, CancellationToken ct)
    {
        var user = await users.FindByEmailAsync(email?.Trim() ?? "");
        if (user is null)
            Flash($"Користувача з email «{email}» не знайдено.");
        else if (await TryAsync(() => subscriptions.GiftAsync(user.Id, days, ct)))
            Flash($"{user.DisplayName} отримує Premium на {days} дн.");
        else
            Flash(ModelState[""]!.Errors[0].ErrorMessage);

        return RedirectToAction(nameof(Index));
    }

    /// <summary>Demo helper: lets the period run out so the state transition (renew / expire) is visible.</summary>
    [HttpPost]
    public async Task<IActionResult> EndPeriod(int id, CancellationToken ct)
    {
        Flash(await TryAsync(() => subscriptions.EndPeriodNowAsync(id, ct))
            ? "Період завершено — стан підписки оновлено."
            : ModelState[""]!.Errors[0].ErrorMessage);
        return RedirectToAction(nameof(Index));
    }
}
