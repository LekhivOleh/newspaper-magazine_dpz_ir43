using CURSOVA.Application.Articles;
using CURSOVA.Application.Authors;
using CURSOVA.Application.Categories;
using CURSOVA.Application.Comments;
using CURSOVA.Application.Sharing;
using CURSOVA.Areas.Admin.Models;
using CURSOVA.Domain.Articles;
using Microsoft.AspNetCore.Mvc;

namespace CURSOVA.Areas.Admin.Controllers;

public class DashboardController(IArticleService articles, ICategoryService categories, IAuthorService authors, ICommentService comments, ISharingFacade sharing) : AdminController
{
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        async Task<int> CountAsync(ArticleStatus status) =>
            (await articles.SearchAllAsync(new ArticleFilter { Status = status, PageSize = 1 }, ct)).TotalCount;

        var recent = await articles.SearchAllAsync(new ArticleFilter { PageSize = 5 }, ct);

        return View(new DashboardPage(
            await CountAsync(ArticleStatus.Published),
            await CountAsync(ArticleStatus.Draft),
            await CountAsync(ArticleStatus.Archived),
            (await categories.ListAsync(ct)).Count,
            (await authors.ListAsync(ct)).Count,
            await comments.CountPendingAsync(ct),
            recent.Items,
            await sharing.GetTopArticlesAsync(5, ct)));
    }
}
