using CURSOVA.Application.Abstractions.Sharing;
using CURSOVA.Application.Articles;
using CURSOVA.Application.Comments;
using CURSOVA.Application.Common;
using CURSOVA.Application.Interactive;
using CURSOVA.Application.Profiles;
using CURSOVA.Application.Sharing;
using CURSOVA.Domain.Articles;
using CURSOVA.Domain.Sharing;
using CURSOVA.Models;
using Microsoft.AspNetCore.Mvc;

namespace CURSOVA.Controllers;

[Route("article")]
public class ArticleController(
    IArticleService articles,
    IPollService polls,
    ICommentService comments,
    ISharingFacade sharing,
    ISavedArticleService saved,
    ICollectionService collections,
    IAppUrlProvider urls) : Controller
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
            await comments.ListForArticleAsync(article.Summary.Id, ct),
            new ShareBarViewModel(
                await sharing.GetLinksAsync(ShareableType.Article, article.Summary.Id, ct),
                (await sharing.GetStatsAsync(ShareableType.Article, article.Summary.Id, ct)).Total,
                urls.ArticleUrl(article.Summary.Slug)),
            await saved.IsSavedAsync(article.Summary.Id, ct),
            await collections.ChoicesForArticleAsync(article.Summary.Id, ct)));
    }
}
