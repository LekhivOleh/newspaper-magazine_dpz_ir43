using CURSOVA.Application.Abstractions.Identity;
using CURSOVA.Application.Interactive;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CURSOVA.Controllers.Api;

[ApiController]
[Route("api/quizzes")]
public class QuizzesController(IQuizService quizzes) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyList<QuizSummaryDto>> List(CancellationToken ct) => quizzes.ListAsync(activeOnly: true, ct);

    [HttpGet("{id:int}")]
    public Task<QuizDto> Get(int id, CancellationToken ct) => quizzes.GetAsync(id, ct);

    [HttpPost("{id:int}/submissions")]
    public Task<QuizResultDto> Submit(int id, SubmitQuizRequest request, CancellationToken ct) =>
        quizzes.SubmitAsync(id, request.OptionIds, ct);

    [HttpGet("{id:int}/result")]
    public async Task<ActionResult<QuizResultDto>> MyResult(int id, CancellationToken ct) =>
        await quizzes.GetMyLastResultAsync(id, ct) is { } result ? result : NotFound();

    [Authorize(Roles = Roles.Admin)]
    [HttpGet("{id:int}/definition")]
    public Task<QuizDefinitionDto> GetDefinition(int id, CancellationToken ct) => quizzes.GetDefinitionAsync(id, ct);

    [Authorize(Roles = Roles.Admin)]
    [HttpPut("{id:int}/definition")]
    public Task<QuizDefinitionDto> SaveDefinition(int id, QuizDefinitionDto definition, CancellationToken ct) =>
        quizzes.SaveDefinitionAsync(id, definition, ct);
}
