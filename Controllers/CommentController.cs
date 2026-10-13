using CURSOVA.Application.Comments;
using CURSOVA.Application.Comments.Moderation;
using CURSOVA.Application.Common;
using CURSOVA.Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CURSOVA.Controllers;

[Authorize]
[AutoValidateAntiforgeryToken]
public class CommentController(ICommentService comments) : Controller
{
    public const string MessageKey = "CommentMessage";

    [HttpPost("/article/{articleId:int}/comments")]
    public async Task<IActionResult> Post(int articleId, string? body, string returnUrl, CancellationToken ct)
    {
        try
        {
            var result = await comments.PostAsync(articleId, body ?? "", ct);
            TempData[MessageKey] = result.Decision switch
            {
                ModerationDecision.Approve => null,
                ModerationDecision.Pending => "Коментар надіслано на модерацію: " + result.Message,
                _ => "Коментар не опубліковано: " + result.Message
            };
            if (result.Decision == ModerationDecision.Reject)
                TempData["CommentDraft"] = body;
        }
        catch (DomainException e)
        {
            TempData[MessageKey] = e.Message;
        }
        catch (NotFoundException)
        {
            return NotFound();
        }

        return Back(returnUrl);
    }

    [HttpPost("/comments/{id:int}/delete")]
    public async Task<IActionResult> Delete(int id, string returnUrl, CancellationToken ct)
    {
        try
        {
            await comments.DeleteAsync(id, ct);
        }
        catch (Exception e) when (e is DomainException or NotFoundException)
        {
            TempData[MessageKey] = e.Message;
        }

        return Back(returnUrl);
    }

    private IActionResult Back(string returnUrl) =>
        LocalRedirect((Url.IsLocalUrl(returnUrl) ? returnUrl : "/") + "#comments");
}
