using CURSOVA.Application.Comments;
using CURSOVA.Application.Comments.Moderation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CURSOVA.Controllers.Api;

[ApiController]
public class CommentsController(ICommentService comments) : ControllerBase
{
    [HttpGet("api/articles/{articleId:int}/comments")]
    public Task<IReadOnlyList<CommentDto>> List(int articleId, CancellationToken ct) => comments.ListForArticleAsync(articleId, ct);

    [Authorize]
    [HttpPost("api/articles/{articleId:int}/comments")]
    [ProducesResponseType<CommentDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<CommentDto>(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> Post(int articleId, PostCommentRequest request, CancellationToken ct)
    {
        var result = await comments.PostAsync(articleId, request.Body, ct);
        return result.Decision switch
        {
            ModerationDecision.Approve => StatusCode(StatusCodes.Status201Created, result.Comment),
            ModerationDecision.Pending => Accepted(result.Comment),
            _ => Problem(title: "Коментар відхилено", detail: result.Message, statusCode: StatusCodes.Status422UnprocessableEntity)
        };
    }

    [Authorize]
    [HttpDelete("api/comments/{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await comments.DeleteAsync(id, ct);
        return NoContent();
    }
}
