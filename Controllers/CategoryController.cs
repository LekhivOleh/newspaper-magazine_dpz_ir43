using CURSOVA.Application.Articles;
using CURSOVA.Application.Categories;
using CURSOVA.Application.Common;
using CURSOVA.Models;
using Microsoft.AspNetCore.Mvc;

namespace CURSOVA.Controllers;

[Route("category")]
public class CategoryController(ICategoryService categories, IArticleService articles) : Controller
{
    private const int PageSize = 9;

    [HttpGet("{slug}")]
    public async Task<IActionResult> Show(string slug, int page = 1, CancellationToken ct = default)
    {
        CategoryDto category;
        try
        {
            category = await categories.GetBySlugAsync(slug, ct);
        }
        catch (NotFoundException)
        {
            return NotFound();
        }

        page = Math.Max(1, page);
        var result = await articles.SearchAsync(new ArticleFilter { CategoryId = category.Id, Page = page, PageSize = PageSize }, ct);
        var pager = new PagerViewModel(page, result.TotalPages, p => Url.Action(nameof(Show), new { slug, page = p })!);

        return View(new CategoryPageViewModel(category, result, pager));
    }
}
