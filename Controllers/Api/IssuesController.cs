using CURSOVA.Application.Issues;
using Microsoft.AspNetCore.Mvc;

namespace CURSOVA.Controllers.Api;

[ApiController]
[Route("api/issues")]
public class IssuesController(IIssueService issues) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyList<IssueSummaryDto>> List(CancellationToken ct) => issues.ListAsync(publishedOnly: true, ct);

    [HttpGet("featured")]
    public async Task<ActionResult<IssueSummaryDto>> Featured(CancellationToken ct) =>
        await issues.GetFeaturedAsync(ct) is { } featured ? featured : NotFound();

    [HttpGet("{slug}")]
    public Task<IssueDetailsDto> Get(string slug, CancellationToken ct) => issues.GetPublishedBySlugAsync(slug, ct);
}
