using CURSOVA.Application.Authors;
using CURSOVA.Application.Common;
using CURSOVA.Areas.Admin.Models;
using Microsoft.AspNetCore.Mvc;

namespace CURSOVA.Areas.Admin.Controllers;

public class AuthorsController(IAuthorService authors) : AdminController
{
    public async Task<IActionResult> Index(CancellationToken ct) => View(await authors.ListAsync(ct));

    public IActionResult Create() => View("Form", new AuthorForm());

    [HttpPost]
    public async Task<IActionResult> Create(AuthorForm form, CancellationToken ct)
    {
        if (!ModelState.IsValid || !await TryAsync(() => authors.CreateAsync(form.ToRequest(), ct)))
            return View("Form", form);

        Flash($"Автора «{form.FullName}» додано.");
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        try
        {
            return View("Form", AuthorForm.From(await authors.GetAsync(id, ct)));
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, AuthorForm form, CancellationToken ct)
    {
        form.Id = id;
        if (!ModelState.IsValid || !await TryAsync(() => authors.UpdateAsync(id, form.ToRequest(), ct)))
            return View("Form", form);

        Flash($"Автора «{form.FullName}» збережено.");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        if (await TryAsync(() => authors.DeleteAsync(id, ct)))
            Flash("Автора видалено.");
        else
            Flash(ModelState[""]!.Errors[0].ErrorMessage);

        return RedirectToAction(nameof(Index));
    }
}
