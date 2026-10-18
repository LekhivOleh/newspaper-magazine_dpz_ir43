using CURSOVA.Application.Subscriptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CURSOVA.Controllers.Api;

[ApiController]
public class SubscriptionController(ISubscriptionService subscriptions) : ControllerBase
{
    [HttpGet("api/subscription/plans")]
    public IReadOnlyList<PlanDto> Plans() => subscriptions.Plans;

    [Authorize]
    [HttpGet("api/me/subscription")]
    public Task<SubscriptionDto> Mine(CancellationToken ct) => subscriptions.GetMineAsync(ct);

    [Authorize]
    [HttpPost("api/me/subscription")]
    public Task<SubscriptionDto> Subscribe(SubscribeRequest request, CancellationToken ct) => subscriptions.SubscribeAsync(request.Plan, ct);

    [Authorize]
    [HttpPost("api/me/subscription/cancel")]
    public Task<SubscriptionDto> Cancel(CancellationToken ct) => subscriptions.CancelAsync(ct);

    [Authorize]
    [HttpPost("api/me/subscription/resume")]
    public Task<SubscriptionDto> Resume(CancellationToken ct) => subscriptions.ResumeAsync(ct);
}
