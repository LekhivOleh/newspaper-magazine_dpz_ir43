using System.Diagnostics;
using CURSOVA.Application.Articles;
using CURSOVA.Models;
using Microsoft.AspNetCore.Mvc;

namespace CURSOVA.Controllers;

public class HomeController(IArticleService articles) : Controller
{
    private const int PageSize = 9;

    public async Task<IActionResult> Index(int page = 1, CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        var result = await articles.SearchAsync(new ArticleFilter { Page = page, PageSize = PageSize }, ct);

        var featured = page == 1 ? result.Items.FirstOrDefault() : null;
        var rest = featured is null ? result.Items : result.Items.Skip(1).ToList();
        var pager = new PagerViewModel(page, result.TotalPages, p => Url.Action(nameof(Index), new { page = p })!);

        return View(new HomeViewModel(featured, rest, pager));
    }

    [HttpGet("/search")]
    public async Task<IActionResult> Search(string? q, int page = 1, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(q))
            return View(new SearchViewModel(q, null, null));

        page = Math.Max(1, page);
        var result = await articles.SearchAsync(new ArticleFilter { Search = q, Page = page, PageSize = PageSize }, ct);
        var pager = new PagerViewModel(page, result.TotalPages, p => Url.Action(nameof(Search), new { q, page = p })!);

        return View(new SearchViewModel(q, result, pager));
    }

    [Route("/status/{code:int}")]
    public IActionResult Status(int code) => code == StatusCodes.Status404NotFound ? View("NotFound") : Error();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() =>
        View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
