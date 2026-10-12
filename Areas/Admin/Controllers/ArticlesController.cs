using CURSOVA.Application.Articles;
using CURSOVA.Application.Authors;
using CURSOVA.Application.Categories;
using CURSOVA.Application.Common;
using CURSOVA.Areas.Admin.Models;
using CURSOVA.Domain.Articles;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CURSOVA.Areas.Admin.Controllers;

public class ArticlesController(IArticleService articles, ICategoryService categories, IAuthorService authors) : AdminController
{
    public async Task<IActionResult> Index(ArticleStatus? status, int? categoryId, string? q, int page = 1, CancellationToken ct = default)
    {
        var result = await articles.SearchAllAsync(new ArticleFilter
        {
            Status = status, CategoryId = categoryId, Search = q, Page = Math.Max(1, page), PageSize = 15
        }, ct);

        return View(new ArticleListPage(result, await categories.ListAsync(ct), status, categoryId, q));
    }

    public async Task<IActionResult> Create(CancellationToken ct) => await FormViewAsync(new ArticleForm(), ct);

    [HttpPost]
    public async Task<IActionResult> Create(ArticleForm form, CancellationToken ct)
    {
        ArticleDetailsDto? created = null;
        if (!ModelState.IsValid || !await TryAsync(async () => created = await articles.CreateAsync(form.ToRequest(), ct)))
            return await FormViewAsync(form, ct);

        Flash("Чернетку створено. Тепер додайте контент.");
        return RedirectToAction(nameof(Content), new { id = created!.Summary.Id });
    }

    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        var article = await FindAsync(id, ct);
        return article is null ? NotFound() : await FormViewAsync(ArticleForm.From(article.Summary), ct);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, ArticleForm form, CancellationToken ct)
    {
        form.Id = id;
        if (!ModelState.IsValid || !await TryAsync(() => articles.UpdateAsync(id, form.ToRequest(), ct)))
            return await FormViewAsync(form, ct);

        Flash("Зміни збережено.");
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Content(int id, CancellationToken ct)
    {
        var article = await FindAsync(id, ct);
        return article is null ? NotFound() : View(article);
    }

    [HttpPost]
    public Task<IActionResult> Publish(int id, string? returnUrl, CancellationToken ct) =>
        RunAndReturnAsync(() => articles.PublishAsync(id, ct), "Статтю опубліковано.", returnUrl);

    [HttpPost]
    public Task<IActionResult> Archive(int id, string? returnUrl, CancellationToken ct) =>
        RunAndReturnAsync(() => articles.ArchiveAsync(id, ct), "Статтю перенесено в архів.", returnUrl);

    [HttpPost]
    public Task<IActionResult> Delete(int id, CancellationToken ct) =>
        RunAndReturnAsync(() => articles.DeleteAsync(id, ct), "Статтю видалено.", null);

    private async Task<IActionResult> RunAndReturnAsync(Func<Task> operation, string successMessage, string? returnUrl)
    {
        Flash(await TryAsync(operation) ? successMessage : ModelState[""]!.Errors[0].ErrorMessage);
        return Url.IsLocalUrl(returnUrl) ? LocalRedirect(returnUrl) : RedirectToAction(nameof(Index));
    }

    private async Task<ArticleDetailsDto?> FindAsync(int id, CancellationToken ct)
    {
        try
        {
            return await articles.GetAsync(id, ct);
        }
        catch (NotFoundException)
        {
            return null;
        }
    }

    private async Task<IActionResult> FormViewAsync(ArticleForm form, CancellationToken ct)
    {
        ViewData["Categories"] = (await categories.ListAsync(ct)).Select(c => new SelectListItem(c.Name, c.Id.ToString())).ToList();
        ViewData["Authors"] = (await authors.ListAsync(ct)).Select(a => new SelectListItem(a.FullName, a.Id.ToString())).ToList();
        return View("Form", form);
    }
}
