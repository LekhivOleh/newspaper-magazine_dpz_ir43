using System.ComponentModel.DataAnnotations;
using CURSOVA.Application.Articles;
using CURSOVA.Application.Authors;
using CURSOVA.Application.Categories;
using CURSOVA.Application.Common;
using CURSOVA.Application.Interactive;
using CURSOVA.Application.Issues;
using CURSOVA.Domain.Articles;
using CURSOVA.Domain.Categories;
using CURSOVA.Domain.Issues;

namespace CURSOVA.Areas.Admin.Models;

public class CategoryForm
{
    public int? Id { get; set; }

    [Required(ErrorMessage = "Вкажіть назву.")]
    [MaxLength(Category.NameMaxLength)]
    [Display(Name = "Назва")]
    public string Name { get; set; } = "";

    [MaxLength(Category.DescriptionMaxLength)]
    [Display(Name = "Опис")]
    public string? Description { get; set; }

    [Display(Name = "Порядок у меню")]
    public int SortOrder { get; set; }

    public static CategoryForm From(CategoryDto c) =>
        new() { Id = c.Id, Name = c.Name, Description = c.Description, SortOrder = c.SortOrder };

    public SaveCategoryRequest ToRequest() => new(Name, Description, SortOrder);
}

public class AuthorForm
{
    public int? Id { get; set; }

    [Required(ErrorMessage = "Вкажіть ім'я.")]
    [MaxLength(120)]
    [Display(Name = "Ім'я та прізвище")]
    public string FullName { get; set; } = "";

    [MaxLength(80)]
    [Display(Name = "Посада")]
    public string? Position { get; set; }

    [MaxLength(1000)]
    [Display(Name = "Біографія")]
    public string? Bio { get; set; }

    [MaxLength(500)]
    [Url(ErrorMessage = "Некоректне посилання.")]
    [Display(Name = "Аватар (URL)")]
    public string? AvatarUrl { get; set; }

    public static AuthorForm From(AuthorDto a) =>
        new() { Id = a.Id, FullName = a.FullName, Position = a.Position, Bio = a.Bio, AvatarUrl = a.AvatarUrl };

    public SaveAuthorRequest ToRequest() => new(FullName, Position, Bio, AvatarUrl);
}

public class ArticleForm
{
    public int? Id { get; set; }

    [Required(ErrorMessage = "Вкажіть заголовок.")]
    [MaxLength(Article.TitleMaxLength)]
    [Display(Name = "Заголовок")]
    public string Title { get; set; } = "";

    [Required(ErrorMessage = "Вкажіть лід.")]
    [MaxLength(Article.LeadMaxLength)]
    [Display(Name = "Лід (короткий опис)")]
    public string Lead { get; set; } = "";

    [Range(1, int.MaxValue, ErrorMessage = "Оберіть рубрику.")]
    [Display(Name = "Рубрика")]
    public int CategoryId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Оберіть автора.")]
    [Display(Name = "Автор")]
    public int AuthorId { get; set; }

    [MaxLength(500)]
    [Url(ErrorMessage = "Некоректне посилання.")]
    [Display(Name = "Обкладинка (URL)")]
    public string? CoverImageUrl { get; set; }

    [Display(Name = "Доступ")]
    public AccessLevel AccessLevel { get; set; } = AccessLevel.Free;

    public static ArticleForm From(ArticleSummaryDto a) => new()
    {
        Id = a.Id, Title = a.Title, Lead = a.Lead, CategoryId = a.Category.Id, AuthorId = a.Author.Id,
        CoverImageUrl = a.CoverImageUrl, AccessLevel = a.AccessLevel
    };

    public SaveArticleRequest ToRequest() => new(Title, Lead, CategoryId, AuthorId, CoverImageUrl, AccessLevel);
}

public record ArticleListPage(
    PagedResult<ArticleSummaryDto> Articles,
    IReadOnlyList<CategoryDto> Categories,
    ArticleStatus? Status,
    int? CategoryId,
    string? Search);

public record DashboardPage(int Published, int Drafts, int Archived, int Categories, int Authors, IReadOnlyList<ArticleSummaryDto> Recent);

public class IssueForm
{
    public int? Id { get; set; }

    [Required(ErrorMessage = "Вкажіть назву.")]
    [MaxLength(Issue.TitleMaxLength)]
    [Display(Name = "Назва випуску")]
    public string Title { get; set; } = "";

    [Required(ErrorMessage = "Вкажіть опис.")]
    [MaxLength(Issue.DescriptionMaxLength)]
    [Display(Name = "Опис")]
    public string Description { get; set; } = "";

    [MaxLength(500)]
    [Url(ErrorMessage = "Некоректне посилання.")]
    [Display(Name = "Обкладинка (URL)")]
    public string? CoverImageUrl { get; set; }

    public static IssueForm From(IssueSummaryDto i) =>
        new() { Id = i.Id, Title = i.Title, Description = i.Description, CoverImageUrl = i.CoverImageUrl };

    public SaveIssueRequest ToRequest() => new(Title, Description, CoverImageUrl);
}

public record SectionOption(int? Id, string Label);

public record IssueStructurePage(IssueDetailsDto Issue, IReadOnlyList<SectionOption> Sections, IReadOnlyList<ArticleSummaryDto> Articles);

public class PollForm
{
    public int? Id { get; set; }

    [Required(ErrorMessage = "Вкажіть назву.")]
    [MaxLength(200)]
    [Display(Name = "Назва (для адмінки та списку)")]
    public string Title { get; set; } = "";

    [MaxLength(1000)]
    [Display(Name = "Опис")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Вкажіть питання.")]
    [MaxLength(300)]
    [Display(Name = "Питання")]
    public string Question { get; set; } = "";

    [Required(ErrorMessage = "Вкажіть варіанти відповідей.")]
    [Display(Name = "Варіанти відповідей (кожен з нового рядка)")]
    public string OptionsText { get; set; } = "";

    [Display(Name = "Дозволити кілька відповідей")]
    public bool AllowMultiple { get; set; }

    [Display(Name = "Показувати під статтею")]
    public int? ArticleId { get; set; }

    public bool HasVotes { get; set; }

    public static PollForm From(PollDto p) => new()
    {
        Id = p.Id, Title = p.Title, Description = p.Description, Question = p.Question,
        OptionsText = string.Join("\n", p.Options.Select(o => o.Text)), AllowMultiple = p.AllowMultiple,
        ArticleId = p.ArticleId, HasVotes = p.ParticipantCount > 0
    };

    public SavePollRequest ToRequest() => new(
        Title, Description, Question,
        OptionsText.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
        AllowMultiple, ArticleId);
}

public class QuizForm
{
    public int? Id { get; set; }

    [Required(ErrorMessage = "Вкажіть назву.")]
    [MaxLength(200)]
    [Display(Name = "Назва тесту")]
    public string Title { get; set; } = "";

    [MaxLength(1000)]
    [Display(Name = "Опис")]
    public string? Description { get; set; }

    [MaxLength(500)]
    [Url(ErrorMessage = "Некоректне посилання.")]
    [Display(Name = "Обкладинка (URL)")]
    public string? CoverImageUrl { get; set; }

    [Display(Name = "Тип тесту")]
    public CURSOVA.Domain.Interactive.Quizzes.ScoringMode ScoringMode { get; set; }

    [Display(Name = "Прив'язати до статті")]
    public int? ArticleId { get; set; }

    public static QuizForm From(QuizSummaryDto q) => new()
    {
        Id = q.Id, Title = q.Title, Description = q.Description, CoverImageUrl = q.CoverImageUrl,
        ScoringMode = q.ScoringMode, ArticleId = q.ArticleId
    };

    public SaveQuizRequest ToRequest() => new(Title, Description, CoverImageUrl, ScoringMode, ArticleId);
}
