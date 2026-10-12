using CURSOVA.Application.Articles;
using CURSOVA.Application.Common;
using CURSOVA.Application.Issues;
using CURSOVA.Areas.Admin.Models;
using Microsoft.AspNetCore.Mvc;

namespace CURSOVA.Areas.Admin.Controllers;

public class IssuesController(IIssueService issues, IArticleService articles) : AdminController
{
    public async Task<IActionResult> Index(CancellationToken ct) => View(await issues.ListAsync(publishedOnly: false, ct));

    public IActionResult Create() => View("Form", new IssueForm());

    [HttpPost]
    public async Task<IActionResult> Create(IssueForm form, CancellationToken ct)
    {
        IssueDetailsDto? created = null;
        if (!ModelState.IsValid || !await TryAsync(async () => created = await issues.CreateAsync(form.ToRequest(), ct)))
            return View("Form", form);

        Flash("Випуск створено. Додайте розділи та статті.");
        return RedirectToAction(nameof(Structure), new { id = created!.Summary.Id });
    }

    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        var issue = await FindAsync(id, ct);
        return issue is null ? NotFound() : View("Form", IssueForm.From(issue.Summary));
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, IssueForm form, CancellationToken ct)
    {
        form.Id = id;
        if (!ModelState.IsValid || !await TryAsync(() => issues.UpdateAsync(id, form.ToRequest(), ct)))
            return View("Form", form);

        Flash("Випуск збережено.");
        return RedirectToAction(nameof(Structure), new { id });
    }

    public async Task<IActionResult> Structure(int id, CancellationToken ct)
    {
        var issue = await FindAsync(id, ct);
        if (issue is null)
            return NotFound();

        var published = await articles.SearchAsync(new ArticleFilter { PageSize = 50 }, ct);
        return View(new IssueStructurePage(issue, SectionOptions(issue.Root).ToList(), published.Items));
    }

    [HttpPost]
    public Task<IActionResult> AddSection(int id, int? parentId, string title, CancellationToken ct) =>
        RunAsync(id, () => issues.AddSectionAsync(id, parentId, title, ct), "Розділ додано.");

    [HttpPost]
    public Task<IActionResult> AddArticle(int id, int? parentId, int articleId, string? editorNote, CancellationToken ct) =>
        RunAsync(id, () => issues.AddArticleAsync(id, parentId, articleId, editorNote, ct), "Статтю додано до випуску.");

    [HttpPost]
    public Task<IActionResult> RemoveComponent(int id, int componentId, CancellationToken ct) =>
        RunAsync(id, () => issues.RemoveComponentAsync(id, componentId, ct), "Елемент прибрано з випуску.");

    [HttpPost]
    public Task<IActionResult> MoveComponent(int id, int componentId, int delta, CancellationToken ct) =>
        RunAsync(id, () => issues.MoveComponentAsync(id, componentId, Math.Sign(delta), ct), null);

    [HttpPost]
    public Task<IActionResult> Publish(int id, CancellationToken ct) =>
        RunAsync(id, () => issues.PublishAsync(id, ct), "Випуск опубліковано.");

    [HttpPost]
    public Task<IActionResult> Unpublish(int id, CancellationToken ct) =>
        RunAsync(id, () => issues.UnpublishAsync(id, ct), "Випуск знято з публікації.");

    [HttpPost]
    public Task<IActionResult> Feature(int id, CancellationToken ct) =>
        RunAsync(id, () => issues.FeatureAsync(id, ct), "Тепер це «Журнал місяця».");

    [HttpPost]
    public Task<IActionResult> Unfeature(int id, CancellationToken ct) =>
        RunAsync(id, () => issues.UnfeatureAsync(id, ct), "Позначку «Журнал місяця» знято.");

    [HttpPost]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        Flash(await TryAsync(() => issues.DeleteAsync(id, ct)) ? "Випуск видалено." : ModelState[""]!.Errors[0].ErrorMessage);
        return RedirectToAction(nameof(Index));
    }

    private async Task<IActionResult> RunAsync(int id, Func<Task> operation, string? successMessage)
    {
        if (await TryAsync(operation))
        {
            if (successMessage is not null)
                Flash(successMessage);
        }
        else
        {
            Flash(ModelState[""]!.Errors[0].ErrorMessage);
        }

        return RedirectToAction(nameof(Structure), new { id });
    }

    private async Task<IssueDetailsDto?> FindAsync(int id, CancellationToken ct)
    {
        try
        {
            return await issues.GetAsync(id, ct);
        }
        catch (NotFoundException)
        {
            return null;
        }
    }

    private static IEnumerable<SectionOption> SectionOptions(SectionNodeDto section, int depth = 0)
    {
        yield return new SectionOption(depth == 0 ? null : section.Id, depth == 0 ? "Корінь випуску" : new string('—', depth) + " " + section.Title);

        foreach (var child in section.Children.OfType<SectionNodeDto>())
        foreach (var option in SectionOptions(child, depth + 1))
            yield return option;
    }
}
