using CURSOVA.Application.Articles;
using CURSOVA.Application.Comments;
using CURSOVA.Application.Common;
using CURSOVA.Application.Interactive;
using CURSOVA.Domain.Articles;
using CURSOVA.Models;
using Microsoft.AspNetCore.Mvc;

namespace CURSOVA.Controllers;

[Route("article")]
public class ArticleController(IArticleService articles, IPollService polls, ICommentService comments) : Controller
{
    [HttpGet("{slug}")]
    public async Task<IActionResult> Show(string slug, CancellationToken ct = default)
    {
        ArticleDetailsDto article;
        try
        {
            article = await articles.GetBySlugAsync(slug, ct);
        }
        catch (NotFoundException)
        {
            return NotFound();
        }

        if (article.Summary.Status != ArticleStatus.Published)
            return NotFound();

        await articles.RegisterViewAsync(article.Summary.Id, ct);

        var related = await articles.SearchAsync(
            new ArticleFilter { CategoryId = article.Summary.Category.Id, PageSize = 4 }, ct);

        return View(new ArticlePageViewModel(
            article,
            related.Items.Where(a => a.Id != article.Summary.Id).Take(3).ToList(),
            await polls.GetForArticleAsync(article.Summary.Id, ct),
            await comments.ListForArticleAsync(article.Summary.Id, ct)));
    }
}
