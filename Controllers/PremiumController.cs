using CURSOVA.Application.Subscriptions;
using CURSOVA.Domain.Common;
using CURSOVA.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CURSOVA.Controllers;

[AutoValidateAntiforgeryToken]
public class PremiumController(ISubscriptionService subscriptions) : Controller
{
    public const string MessageKey = "SubscriptionMessage";

    [HttpGet("/premium")]
    public async Task<IActionResult> Index(CancellationToken ct) =>
        View(new PremiumPageViewModel(
            subscriptions.Plans,
            User.Identity?.IsAuthenticated == true ? await subscriptions.GetMineAsync(ct) : null));

    [Authorize]
    [HttpPost("/premium/subscribe")]
    public Task<IActionResult> Subscribe(string plan, CancellationToken ct) =>
        RunAsync(() => subscriptions.SubscribeAsync(plan, ct), "Дякуємо! Premium активовано (демо-оплата, кошти не списано).");

    [Authorize]
    [HttpPost("/premium/cancel")]
    public Task<IActionResult> Cancel(CancellationToken ct) =>
        RunAsync(() => subscriptions.CancelAsync(ct), "Автопродовження вимкнено. Premium діятиме до кінця оплаченого періоду.");

    [Authorize]
    [HttpPost("/premium/resume")]
    public Task<IActionResult> Resume(CancellationToken ct) =>
        RunAsync(() => subscriptions.ResumeAsync(ct), "Автопродовження знову увімкнено.");

    private async Task<IActionResult> RunAsync(Func<Task> operation, string success)
    {
        try
        {
            await operation();
            TempData[MessageKey] = success;
        }
        catch (DomainException e)
        {
            TempData[MessageKey] = e.Message;
        }

        return LocalRedirect("/profile/subscription");
    }
}
