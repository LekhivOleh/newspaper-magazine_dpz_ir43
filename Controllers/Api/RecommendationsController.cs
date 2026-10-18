using CURSOVA.Application.Recommendations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CURSOVA.Controllers.Api;

public record SetInterestsRequest(IReadOnlyList<int> CategoryIds);

[ApiController]
public class RecommendationsController(IRecommendationService recommendations) : ControllerBase
{
    /// <summary>Works for guests too — they simply get the popularity strategy.</summary>
    [HttpGet("api/recommendations")]
    public Task<RecommendationsDto> Get(string? strategy, int take = 6, CancellationToken ct = default) =>
        recommendations.RecommendAsync(take, strategy, ct);

    [Authorize]
    [HttpGet("api/me/interests")]
    public Task<IReadOnlyList<InterestDto>> Interests(CancellationToken ct) => recommendations.GetInterestsAsync(ct);

    [Authorize]
    [HttpPut("api/me/interests")]
    public async Task<IActionResult> SetInterests(SetInterestsRequest request, CancellationToken ct)
    {
        await recommendations.SetInterestsAsync(request.CategoryIds ?? [], ct);
        return NoContent();
    }
}
