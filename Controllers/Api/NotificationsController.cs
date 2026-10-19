using CURSOVA.Application.Common;
using CURSOVA.Application.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CURSOVA.Controllers.Api;

[ApiController]
[Authorize]
[Route("api/me/notifications")]
public class NotificationsController(INotificationService notifications) : ControllerBase
{
    [HttpGet("")]
    public Task<PagedResult<NotificationDto>> List(int page = 1, CancellationToken ct = default) =>
        notifications.ListMineAsync(page, ct);

    [HttpGet("unread-count")]
    public async Task<object> UnreadCount(CancellationToken ct) => new { count = await notifications.CountUnreadAsync(ct) };

    [HttpPost("{id:int}/read")]
    public Task<NotificationDto> MarkRead(int id, CancellationToken ct) => notifications.MarkReadAsync(id, ct);

    [HttpPost("read-all")]
    public async Task<object> MarkAllRead(CancellationToken ct) => new { marked = await notifications.MarkAllReadAsync(ct) };
}
