using CURSOVA.Application.Notifications;
using CURSOVA.Models;
using Microsoft.AspNetCore.Mvc;

namespace CURSOVA.ViewComponents;

public class NotificationBellViewComponent(INotificationService notifications) : ViewComponent
{
    private const int Latest = 5;

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var ct = HttpContext.RequestAborted;
        var page = await notifications.ListMineAsync(1, ct);
        return View(new NotificationBellViewModel(await notifications.CountUnreadAsync(ct), page.Items.Take(Latest).ToList()));
    }
}
