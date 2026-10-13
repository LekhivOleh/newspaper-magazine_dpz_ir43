using CURSOVA.Application.Articles;
using CURSOVA.Application.Common;
using CURSOVA.Application.Interactive;
using CURSOVA.Areas.Admin.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CURSOVA.Areas.Admin.Controllers;

public class QuizzesController(IQuizService quizzes, IArticleService articles) : AdminController
{
    public async Task<IActionResult> Index(CancellationToken ct) => View(await quizzes.ListAsync(activeOnly: false, ct));

    public Task<IActionResult> Create(CancellationToken ct) => FormViewAsync(new QuizForm(), ct);

    [HttpPost]
    public async Task<IActionResult> Create(QuizForm form, CancellationToken ct)
    {
        QuizSummaryDto? created = null;
        if (!ModelState.IsValid || !await TryAsync(async () => created = await quizzes.CreateAsync(form.ToRequest(), ct)))
            return await FormViewAsync(form, ct);

        Flash("Тест створено. Тепер додайте питання та результати.");
        return RedirectToAction(nameof(Definition), new { id = created!.Id });
    }

    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        var quiz = await FindAsync(id, ct);
        return quiz is null ? NotFound() : await FormViewAsync(QuizForm.From(quiz.Summary), ct);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, QuizForm form, CancellationToken ct)
    {
        form.Id = id;
        if (!ModelState.IsValid || !await TryAsync(() => quizzes.UpdateAsync(id, form.ToRequest(), ct)))
            return await FormViewAsync(form, ct);

        Flash("Тест збережено.");
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Definition(int id, CancellationToken ct)
    {
        var quiz = await FindAsync(id, ct);
        if (quiz is null)
            return NotFound();

        ViewData["Definition"] = await quizzes.GetDefinitionAsync(id, ct);
        return View(quiz.Summary);
    }

    [HttpPost]
    public async Task<IActionResult> SetActive(int id, bool active, CancellationToken ct)
    {
        if (await TryAsync(() => quizzes.SetActiveAsync(id, active, ct)))
            Flash(active ? "Тест відкрито." : "Тест приховано.");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        if (await TryAsync(() => quizzes.DeleteAsync(id, ct)))
            Flash("Тест видалено.");
        return RedirectToAction(nameof(Index));
    }

    private async Task<QuizDto?> FindAsync(int id, CancellationToken ct)
    {
        try
        {
            return await quizzes.GetAsync(id, ct);
        }
        catch (NotFoundException)
        {
            return null;
        }
    }

    private async Task<IActionResult> FormViewAsync(QuizForm form, CancellationToken ct)
    {
        var published = await articles.SearchAsync(new ArticleFilter { PageSize = 50 }, ct);
        ViewData["Articles"] = published.Items.Select(a => new SelectListItem(a.Title, a.Id.ToString())).ToList();
        return View("Form", form);
    }
}
