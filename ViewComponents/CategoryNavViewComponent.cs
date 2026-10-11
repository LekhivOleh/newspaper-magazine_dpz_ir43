using CURSOVA.Application.Categories;
using Microsoft.AspNetCore.Mvc;

namespace CURSOVA.ViewComponents;

public class CategoryNavViewComponent(ICategoryService categories) : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync() =>
        View(await categories.ListAsync(HttpContext.RequestAborted));
}
