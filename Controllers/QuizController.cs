using CURSOVA.Application.Common;
using CURSOVA.Application.Interactive;
using CURSOVA.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace CURSOVA.Controllers;

[Route("quizzes")]
public class QuizController(IQuizService quizzes) : Controller
{
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Show(int id, CancellationToken ct)
    {
        try
        {
            var quiz = await quizzes.GetAsync(id, ct);
            ViewData["LastResult"] = await quizzes.GetMyLastResultAsync(id, ct);
            return View(quiz);
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit(int id, CancellationToken ct)
    {
        // Each question posts its choice as "q{questionId}" — collect all selected option ids.
        var optionIds = Request.Form
            .Where(f => f.Key.StartsWith('q'))
            .SelectMany(f => f.Value)
            .Select(v => int.TryParse(v, out var optionId) ? optionId : 0)
            .Where(optionId => optionId > 0)
            .ToList();

        try
        {
            await quizzes.SubmitAsync(id, optionIds, ct);
            return RedirectToAction(nameof(Result), new { id });
        }
        catch (DomainException e)
        {
            TempData["QuizError"] = e.Message;
            return RedirectToAction(nameof(Show), new { id });
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
    }

    [HttpGet("{id:int}/result")]
    public async Task<IActionResult> Result(int id, CancellationToken ct)
    {
        try
        {
            var result = await quizzes.GetMyLastResultAsync(id, ct);
            return result is null ? RedirectToAction(nameof(Show), new { id }) : View(result);
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
    }
}
