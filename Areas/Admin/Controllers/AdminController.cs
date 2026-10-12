using CURSOVA.Application.Abstractions.Identity;
using CURSOVA.Application.Common;
using CURSOVA.Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CURSOVA.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = Roles.Admin)]
[AutoValidateAntiforgeryToken]
public abstract class AdminController : Controller
{
    protected const string FlashKey = "AdminFlash";

    protected void Flash(string message) => TempData[FlashKey] = message;

    /// <summary>Runs a business operation and turns domain/conflict errors into model state errors.</summary>
    protected async Task<bool> TryAsync(Func<Task> operation)
    {
        try
        {
            await operation();
            return true;
        }
        catch (Exception e) when (e is DomainException or ConflictException or NotFoundException)
        {
            ModelState.AddModelError("", e.Message);
            return false;
        }
    }
}
