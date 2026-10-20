using CURSOVA.Application.Abstractions.Identity;
using CURSOVA.Application.Common;
using CURSOVA.Application.Notifications;
using CURSOVA.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CURSOVA.Controllers;

[Authorize]
[AutoValidateAntiforgeryToken]
public class NotificationController(INotificationService notifications, ICurrentUser currentUser) : Controller
{
    [HttpGet("/profile/notifications")]
    public async Task<IActionResult> Notifications(int page = 1, CancellationToken ct = default)
    {
        var result = await notifications.ListMineAsync(page, ct);
        var pager = new PagerViewModel(result.Page, result.TotalPages, p => Url.Action(nameof(Notifications), new { page = p })!);
        return View(new NotificationsPageViewModel(currentUser.DisplayName ?? "Читач", result, await notifications.CountUnreadAsync(ct), pager));
    }

    [HttpPost("/notifications/{id:int}/open")]
    public async Task<IActionResult> Open(int id, CancellationToken ct)
    {
        try
        {
            var notification = await notifications.MarkReadAsync(id, ct);
            return Url.IsLocalUrl(notification.Link) ? LocalRedirect(notification.Link) : RedirectToAction(nameof(Notifications));
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("/notifications/read-all")]
    public async Task<IActionResult> ReadAll(string? returnUrl, CancellationToken ct)
    {
        await notifications.MarkAllReadAsync(ct);
        return Url.IsLocalUrl(returnUrl) ? LocalRedirect(returnUrl) : RedirectToAction(nameof(Notifications));
    }
}
