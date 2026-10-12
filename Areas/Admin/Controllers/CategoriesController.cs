using CURSOVA.Application.Categories;
using CURSOVA.Application.Common;
using CURSOVA.Areas.Admin.Models;
using Microsoft.AspNetCore.Mvc;

namespace CURSOVA.Areas.Admin.Controllers;

public class CategoriesController(ICategoryService categories) : AdminController
{
    public async Task<IActionResult> Index(CancellationToken ct) => View(await categories.ListAsync(ct));

    public IActionResult Create() => View("Form", new CategoryForm());

    [HttpPost]
    public async Task<IActionResult> Create(CategoryForm form, CancellationToken ct)
    {
        if (!ModelState.IsValid || !await TryAsync(() => categories.CreateAsync(form.ToRequest(), ct)))
            return View("Form", form);

        Flash($"Рубрику «{form.Name}» створено.");
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        try
        {
            return View("Form", CategoryForm.From(await categories.GetAsync(id, ct)));
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, CategoryForm form, CancellationToken ct)
    {
        form.Id = id;
        if (!ModelState.IsValid || !await TryAsync(() => categories.UpdateAsync(id, form.ToRequest(), ct)))
            return View("Form", form);

        Flash($"Рубрику «{form.Name}» збережено.");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        if (await TryAsync(() => categories.DeleteAsync(id, ct)))
            Flash("Рубрику видалено.");
        else
            Flash(ModelState[""]!.Errors[0].ErrorMessage);

        return RedirectToAction(nameof(Index));
    }
}
