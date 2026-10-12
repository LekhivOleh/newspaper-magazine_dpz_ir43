using CURSOVA.Application.Articles;
using CURSOVA.Application.Common;
using CURSOVA.Application.Interactive;
using CURSOVA.Areas.Admin.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CURSOVA.Areas.Admin.Controllers;

public class PollsController(IPollService polls, IArticleService articles) : AdminController
{
    public async Task<IActionResult> Index(CancellationToken ct) => View(await polls.ListAsync(activeOnly: false, ct));

    public Task<IActionResult> Create(CancellationToken ct) => FormViewAsync(new PollForm(), ct);

    [HttpPost]
    public async Task<IActionResult> Create(PollForm form, CancellationToken ct)
    {
        if (!ModelState.IsValid || !await TryAsync(() => polls.CreateAsync(form.ToRequest(), ct)))
            return await FormViewAsync(form, ct);

        Flash("Опитування створено.");
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        try
        {
            return await FormViewAsync(PollForm.From(await polls.GetAsync(id, ct)), ct);
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, PollForm form, CancellationToken ct)
    {
        form.Id = id;
        if (!ModelState.IsValid || !await TryAsync(() => polls.UpdateAsync(id, form.ToRequest(), ct)))
            return await FormViewAsync(form, ct);

        Flash("Опитування збережено.");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> SetActive(int id, bool active, CancellationToken ct)
    {
        if (await TryAsync(() => polls.SetActiveAsync(id, active, ct)))
            Flash(active ? "Опитування відкрито." : "Опитування закрито.");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        if (await TryAsync(() => polls.DeleteAsync(id, ct)))
            Flash("Опитування видалено.");
        return RedirectToAction(nameof(Index));
    }

    private async Task<IActionResult> FormViewAsync(PollForm form, CancellationToken ct)
    {
        var published = await articles.SearchAsync(new ArticleFilter { PageSize = 50 }, ct);
        ViewData["Articles"] = published.Items.Select(a => new SelectListItem(a.Title, a.Id.ToString())).ToList();
        return View("Form", form);
    }
}
