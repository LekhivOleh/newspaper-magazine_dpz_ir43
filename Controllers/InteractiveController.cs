using CURSOVA.Application.Interactive;
using CURSOVA.Models;
using Microsoft.AspNetCore.Mvc;

namespace CURSOVA.Controllers;

public class InteractiveController(IQuizService quizzes, IPollService polls) : Controller
{
    [HttpGet("/interactive")]
    public async Task<IActionResult> Index(CancellationToken ct) =>
        View(new InteractiveHubViewModel(
            await quizzes.ListAsync(activeOnly: true, ct),
            await polls.ListAsync(activeOnly: false, ct)));

    [HttpGet("/quizzes")]
    public IActionResult Quizzes() => Redirect("/interactive#quizzes");
}
