using System.ComponentModel.DataAnnotations;
using CURSOVA.Domain.Interactive;
using CURSOVA.Domain.Interactive.Quizzes;

namespace CURSOVA.Application.Interactive;

public record QuizSummaryDto(
    int Id,
    string Title,
    string? Description,
    string? CoverImageUrl,
    ScoringMode ScoringMode,
    int QuestionCount,
    int ParticipantCount,
    bool IsActive,
    int? ArticleId);

// Public shape: no correct answers, outcome keys or points leak to the reader.
public record QuizDto(QuizSummaryDto Summary, IReadOnlyList<QuizQuestionDto> Questions);

public record QuizQuestionDto(int Id, string Text, IReadOnlyList<QuizOptionDto> Options);

public record QuizOptionDto(int Id, string Text);

public record QuizAnswerReviewDto(string Question, string YourAnswer, string CorrectAnswer, bool IsCorrect);

public record QuizResultDto(
    int QuizId,
    string QuizTitle,
    ScoringMode ScoringMode,
    string OutcomeTitle,
    string? OutcomeDescription,
    int Score,
    int MaxScore,
    DateTime SubmittedAt,
    IReadOnlyList<QuizAnswerReviewDto> Review)
{
    public int Percent => MaxScore == 0 ? 0 : Score * 100 / MaxScore;
}

public record SubmitQuizRequest([Required] IReadOnlyList<int> OptionIds);

public record SaveQuizRequest(string Title, string? Description, string? CoverImageUrl, ScoringMode ScoringMode, int? ArticleId);

// Admin shape used by the definition editor.
public record QuizDefinitionDto(
    ScoringMode ScoringMode,
    IReadOnlyList<QuestionDefinitionDto> Questions,
    IReadOnlyList<OutcomeDefinitionDto> Outcomes);

public record QuestionDefinitionDto(string Text, IReadOnlyList<OptionDefinitionDto> Options);

public record OptionDefinitionDto(string Text, bool IsCorrect, string? OutcomeKey, int Points);

public record OutcomeDefinitionDto(string Key, string Title, string? Description, int Threshold);

public static class QuizMapper
{
    public static QuizSummaryDto ToSummary(this Quiz q) => new(
        q.Id, q.Title, q.Description, q.CoverImageUrl, q.ScoringMode, q.Questions.Count, q.ParticipantCount, q.IsActive, q.ArticleId);

    public static QuizDto ToDto(this Quiz q) => new(
        q.ToSummary(),
        q.Questions.Select(question => new QuizQuestionDto(
            question.Id, question.Text,
            question.Options.Select(o => new QuizOptionDto(o.Id, o.Text)).ToList())).ToList());

    public static QuizDefinitionDto ToDefinition(this Quiz q) => new(
        q.ScoringMode,
        q.Questions.Select(question => new QuestionDefinitionDto(
            question.Text,
            question.Options.Select(o => new OptionDefinitionDto(o.Text, o.IsCorrect, o.OutcomeKey, o.Points)).ToList())).ToList(),
        q.Outcomes.Select(o => new OutcomeDefinitionDto(o.Key, o.Title, o.Description, o.Threshold)).ToList());

    public static QuizResultDto ToResult(this Quiz quiz, Submission submission)
    {
        var outcome = quiz.FindOutcome(submission.ResultKey);
        var review = quiz.ScoringMode != ScoringMode.CorrectAnswers
            ? []
            : quiz.Questions.Select(question =>
            {
                var yours = question.Options.FirstOrDefault(o => submission.SelectedOptionIds.Contains(o.Id));
                var correct = question.Options.First(o => o.IsCorrect);
                return new QuizAnswerReviewDto(question.Text, yours?.Text ?? "—", correct.Text, yours == correct);
            }).ToList();

        return new QuizResultDto(
            quiz.Id, quiz.Title, quiz.ScoringMode,
            outcome?.Title ?? "Результат", outcome?.Description,
            submission.Score ?? 0, submission.MaxScore ?? 0, submission.SubmittedAt, review);
    }
}
