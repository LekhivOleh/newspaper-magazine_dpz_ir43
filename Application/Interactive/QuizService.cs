using CURSOVA.Application.Abstractions.Identity;
using CURSOVA.Application.Abstractions.Persistence;
using CURSOVA.Application.Common;
using CURSOVA.Domain.Interactive;
using CURSOVA.Domain.Interactive.Quizzes;

namespace CURSOVA.Application.Interactive;

public interface IQuizService
{
    Task<IReadOnlyList<QuizSummaryDto>> ListAsync(bool activeOnly, CancellationToken ct = default);
    Task<QuizDto> GetAsync(int id, CancellationToken ct = default);
    Task<QuizResultDto> SubmitAsync(int id, IReadOnlyList<int> optionIds, CancellationToken ct = default);
    Task<QuizResultDto?> GetMyLastResultAsync(int id, CancellationToken ct = default);

    Task<QuizSummaryDto> CreateAsync(SaveQuizRequest request, CancellationToken ct = default);
    Task<QuizSummaryDto> UpdateAsync(int id, SaveQuizRequest request, CancellationToken ct = default);
    Task<QuizDefinitionDto> GetDefinitionAsync(int id, CancellationToken ct = default);
    Task<QuizDefinitionDto> SaveDefinitionAsync(int id, QuizDefinitionDto definition, CancellationToken ct = default);
    Task SetActiveAsync(int id, bool active, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}

public class QuizService(IUnitOfWork uow, IParticipantContext participant) : IQuizService
{
    public async Task<IReadOnlyList<QuizSummaryDto>> ListAsync(bool activeOnly, CancellationToken ct = default) =>
        (await uow.Interactive.ListQuizzesAsync(activeOnly, ct)).Select(q => q.ToSummary()).ToList();

    public async Task<QuizDto> GetAsync(int id, CancellationToken ct = default) => (await LoadAsync(id, ct)).ToDto();

    public async Task<QuizResultDto> SubmitAsync(int id, IReadOnlyList<int> optionIds, CancellationToken ct = default)
    {
        var quiz = await LoadAsync(id, ct);
        var previous = await uow.Interactive.GetLatestSubmissionAsync(id, participant.Key, ct);

        var submission = quiz.Submit(new Participant(participant.Key, participant.UserId, previous is not null), optionIds);
        uow.Interactive.AddSubmission(submission);
        await uow.SaveChangesAsync(ct);

        return quiz.ToResult(submission);
    }

    public async Task<QuizResultDto?> GetMyLastResultAsync(int id, CancellationToken ct = default)
    {
        var quiz = await LoadAsync(id, ct);
        var last = await uow.Interactive.GetLatestSubmissionAsync(id, participant.Key, ct);
        return last is null ? null : quiz.ToResult(last);
    }

    public async Task<QuizSummaryDto> CreateAsync(SaveQuizRequest request, CancellationToken ct = default)
    {
        var quiz = new Quiz(request.Title, request.ScoringMode, request.Description, request.CoverImageUrl, request.ArticleId);
        uow.Interactive.Add(quiz);
        await uow.SaveChangesAsync(ct);
        return quiz.ToSummary();
    }

    public async Task<QuizSummaryDto> UpdateAsync(int id, SaveQuizRequest request, CancellationToken ct = default)
    {
        var quiz = await LoadAsync(id, ct);
        quiz.UpdateDetails(request.Title, request.Description, request.ArticleId);
        quiz.UpdateCover(request.CoverImageUrl);

        await uow.SaveChangesAsync(ct);
        return quiz.ToSummary();
    }

    public async Task<QuizDefinitionDto> GetDefinitionAsync(int id, CancellationToken ct = default) =>
        (await LoadAsync(id, ct)).ToDefinition();

    public async Task<QuizDefinitionDto> SaveDefinitionAsync(int id, QuizDefinitionDto definition, CancellationToken ct = default)
    {
        var quiz = await LoadAsync(id, ct);
        var (questions, outcomes) = ToSpecs(definition);
        quiz.Define(definition.ScoringMode, questions, outcomes);

        await uow.SaveChangesAsync(ct);
        return quiz.ToDefinition();
    }

    public async Task SetActiveAsync(int id, bool active, CancellationToken ct = default)
    {
        var quiz = await LoadAsync(id, ct);
        if (active)
            quiz.Open();
        else
            quiz.Close();
        await uow.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        uow.Interactive.Remove(await LoadAsync(id, ct));
        await uow.SaveChangesAsync(ct);
    }

    private async Task<Quiz> LoadAsync(int id, CancellationToken ct) =>
        await uow.Interactive.GetQuizAsync(id, ct) ?? throw NotFoundException.For<Quiz>(id);

    private static (IReadOnlyList<QuestionSpec> Questions, IReadOnlyList<OutcomeSpec> Outcomes) ToSpecs(QuizDefinitionDto d) => (
        (d.Questions ?? []).Select(q => new QuestionSpec(q.Text,
            (q.Options ?? []).Select(o => new OptionSpec(o.Text, o.IsCorrect, o.OutcomeKey, o.Points)).ToList())).ToList(),
        (d.Outcomes ?? []).Select(o => new OutcomeSpec(o.Key, o.Title, o.Description, o.Threshold)).ToList());
}
