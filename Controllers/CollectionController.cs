using CURSOVA.Application.Common;
using CURSOVA.Application.Profiles;
using CURSOVA.Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CURSOVA.Controllers;

[AutoValidateAntiforgeryToken]
public class CollectionController(ICollectionService collections) : Controller
{
    public const string MessageKey = "CollectionMessage";

    [HttpGet("/collections")]
    public async Task<IActionResult> Index(CancellationToken ct) => View(await collections.ListPublicAsync(ct));

    [HttpGet("/collections/{id:int}")]
    public async Task<IActionResult> Show(int id, CancellationToken ct)
    {
        try
        {
            return View(await collections.GetAsync(id, ct));
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
    }

    /// <summary>Creates a collection; when <paramref name="articleId"/> is given the article goes straight into it.</summary>
    [Authorize]
    [HttpPost("/collections")]
    public Task<IActionResult> Create(string name, string? description, bool isPublic, int? articleId, string? returnUrl, CancellationToken ct) =>
        RunAsync(async () =>
        {
            var created = await collections.CreateAsync(new SaveCollectionRequest(name ?? "", description, isPublic), ct);
            if (articleId is > 0)
                await collections.AddArticleAsync(created.Id, articleId.Value, null, ct);
            return $"Підбірку «{created.Name}» створено.";
        }, returnUrl);

    [Authorize]
    [HttpPost("/collections/{id:int}/edit")]
    public Task<IActionResult> Edit(int id, string name, string? description, bool isPublic, CancellationToken ct) =>
        RunAsync(async () =>
        {
            await collections.UpdateAsync(id, new SaveCollectionRequest(name ?? "", description, isPublic), ct);
            return "Зміни збережено.";
        }, $"/collections/{id}");

    [Authorize]
    [HttpPost("/collections/{id:int}/delete")]
    public Task<IActionResult> Delete(int id, CancellationToken ct) =>
        RunAsync(async () =>
        {
            await collections.DeleteAsync(id, ct);
            return "Підбірку видалено.";
        }, "/profile/collections");

    [Authorize]
    [HttpPost("/collections/add")]
    public Task<IActionResult> AddItem(int collectionId, int articleId, string? note, string? returnUrl, CancellationToken ct) =>
        RunAsync(async () =>
        {
            await collections.AddArticleAsync(collectionId, articleId, note, ct);
            return "Статтю додано до підбірки.";
        }, returnUrl);

    [Authorize]
    [HttpPost("/collections/{id:int}/items/{articleId:int}/remove")]
    public Task<IActionResult> RemoveItem(int id, int articleId, CancellationToken ct) =>
        RunAsync(async () =>
        {
            await collections.RemoveArticleAsync(id, articleId, ct);
            return "Статтю прибрано з підбірки.";
        }, $"/collections/{id}");

    [Authorize]
    [HttpPost("/collections/{id:int}/items/{articleId:int}/move")]
    public Task<IActionResult> MoveItem(int id, int articleId, int delta, CancellationToken ct) =>
        RunAsync(async () =>
        {
            await collections.MoveArticleAsync(id, articleId, delta, ct);
            return null;
        }, $"/collections/{id}");

    [Authorize]
    [HttpPost("/collections/{id:int}/copy")]
    public async Task<IActionResult> Copy(int id, CancellationToken ct)
    {
        try
        {
            var copyId = await collections.CopyAsync(id, ct);
            TempData[MessageKey] = "Копію збережено у ваших підбірках — тепер її можна редагувати.";
            return LocalRedirect($"/collections/{copyId}");
        }
        catch (Exception e) when (e is DomainException or NotFoundException)
        {
            TempData[MessageKey] = e.Message;
            return LocalRedirect($"/collections/{id}");
        }
    }

    private async Task<IActionResult> RunAsync(Func<Task<string?>> operation, string? returnUrl)
    {
        try
        {
            TempData[MessageKey] = await operation();
        }
        catch (Exception e) when (e is DomainException or NotFoundException)
        {
            TempData[MessageKey] = e.Message;
        }

        return LocalRedirect(Url.IsLocalUrl(returnUrl) ? returnUrl : "/profile/collections");
    }
}
