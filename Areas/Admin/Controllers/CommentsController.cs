using CURSOVA.Application.Comments;
using CURSOVA.Domain.Comments;
using Microsoft.AspNetCore.Mvc;

namespace CURSOVA.Areas.Admin.Controllers;

public class CommentsController(ICommentService comments) : AdminController
{
    public async Task<IActionResult> Index(CommentStatus? status = CommentStatus.Pending, bool all = false, int page = 1, CancellationToken ct = default)
    {
        var filter = all ? null : status;
        ViewData["Status"] = filter;
        return View(await comments.ListForModerationAsync(filter, page, ct));
    }

    [HttpPost]
    public async Task<IActionResult> Approve(int id, string? returnUrl, CancellationToken ct)
    {
        if (await TryAsync(() => comments.ApproveAsync(id, ct)))
            Flash("Коментар опубліковано.");
        return Back(returnUrl);
    }

    [HttpPost]
    public async Task<IActionResult> Reject(int id, string? reason, string? returnUrl, CancellationToken ct)
    {
        if (await TryAsync(() => comments.RejectAsync(id, reason, ct)))
            Flash("Коментар відхилено.");
        return Back(returnUrl);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id, string? returnUrl, CancellationToken ct)
    {
        if (await TryAsync(() => comments.DeleteAsync(id, ct)))
            Flash("Коментар видалено.");
        return Back(returnUrl);
    }

    private IActionResult Back(string? returnUrl) =>
        Url.IsLocalUrl(returnUrl) ? LocalRedirect(returnUrl) : RedirectToAction(nameof(Index));
}
