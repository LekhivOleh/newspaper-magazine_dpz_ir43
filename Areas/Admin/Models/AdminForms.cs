using System.ComponentModel.DataAnnotations;
using CURSOVA.Application.Articles;
using CURSOVA.Application.Authors;
using CURSOVA.Application.Categories;
using CURSOVA.Application.Common;
using CURSOVA.Domain.Articles;
using CURSOVA.Domain.Categories;

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
