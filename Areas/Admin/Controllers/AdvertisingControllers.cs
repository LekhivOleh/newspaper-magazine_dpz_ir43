using CURSOVA.Application.Advertising;
using CURSOVA.Application.Categories;
using CURSOVA.Application.Common;
using CURSOVA.Areas.Admin.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CURSOVA.Areas.Admin.Controllers;

public class PartnersController(IAdvertisingService advertising) : AdminController
{
    public async Task<IActionResult> Index(CancellationToken ct) => View(await advertising.ListPartnersAsync(ct));

    public IActionResult Create() => View("Form", new PartnerForm());

    [HttpPost]
    public Task<IActionResult> Create(PartnerForm form, CancellationToken ct) => SaveAsync(null, form, ct);

    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        try
        {
            return View("Form", PartnerForm.From(await advertising.GetPartnerAsync(id, ct)));
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost]
    public Task<IActionResult> Edit(int id, PartnerForm form, CancellationToken ct) => SaveAsync(id, form, ct);

    [HttpPost]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        Flash(await TryAsync(() => advertising.DeletePartnerAsync(id, ct)) ? "Партнера видалено." : ModelState[""]!.Errors[0].ErrorMessage);
        return RedirectToAction(nameof(Index));
    }

    private async Task<IActionResult> SaveAsync(int? id, PartnerForm form, CancellationToken ct)
    {
        form.Id = id;
        if (!ModelState.IsValid || !await TryAsync(() => advertising.SavePartnerAsync(id, form.ToRequest(), ct)))
            return View("Form", form);

        Flash($"Партнера «{form.Name}» збережено.");
        return RedirectToAction(nameof(Index));
    }
}

public class AdsController(IAdvertisingService advertising, ICategoryService categories) : AdminController
{
    public async Task<IActionResult> Index(CancellationToken ct) => View(await advertising.ListAdsAsync(ct));

    public Task<IActionResult> Create(CancellationToken ct) => FormViewAsync(new AdForm(), ct);

    [HttpPost]
    public Task<IActionResult> Create(AdForm form, CancellationToken ct) => SaveAsync(null, form, ct);

    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        try
        {
            return await FormViewAsync(AdForm.From(await advertising.GetAdAsync(id, ct)), ct);
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost]
    public Task<IActionResult> Edit(int id, AdForm form, CancellationToken ct) => SaveAsync(id, form, ct);

    [HttpPost]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        if (await TryAsync(() => advertising.DeleteAdAsync(id, ct)))
            Flash("Оголошення видалено.");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Promote(int id, CancellationToken ct)
    {
        Flash(await TryAsync(() => advertising.LaunchPromotionAsync(id, ct))
            ? "Пропозицію розіслано читачам без Premium — вони побачать її у сповіщеннях."
            : ModelState[""]!.Errors[0].ErrorMessage);
        return RedirectToAction(nameof(Index));
    }

    private async Task<IActionResult> SaveAsync(int? id, AdForm form, CancellationToken ct)
    {
        form.Id = id;
        if (!ModelState.IsValid || !await TryAsync(() => advertising.SaveAdAsync(id, form.ToRequest(), ct)))
            return await FormViewAsync(form, ct);

        Flash("Оголошення збережено.");
        return RedirectToAction(nameof(Index));
    }

    private async Task<IActionResult> FormViewAsync(AdForm form, CancellationToken ct)
    {
        ViewData["Partners"] = (await advertising.ListPartnersAsync(ct)).Select(p => new SelectListItem(p.Name, p.Id.ToString())).ToList();
        ViewData["Categories"] = (await categories.ListAsync(ct)).Select(c => new SelectListItem(c.Name, c.Id.ToString())).ToList();
        return View("Form", form);
    }
}
