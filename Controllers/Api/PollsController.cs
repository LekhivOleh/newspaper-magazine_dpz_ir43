using CURSOVA.Application.Interactive;
using Microsoft.AspNetCore.Mvc;

namespace CURSOVA.Controllers.Api;

[ApiController]
[Route("api/polls")]
public class PollsController(IPollService polls) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyList<PollDto>> List(CancellationToken ct) => polls.ListAsync(activeOnly: true, ct);

    [HttpGet("{id:int}")]
    public Task<PollDto> Get(int id, CancellationToken ct) => polls.GetAsync(id, ct);

    [HttpPost("{id:int}/votes")]
    public Task<PollDto> Vote(int id, VoteRequest request, CancellationToken ct) => polls.VoteAsync(id, request.OptionIds, ct);
}
