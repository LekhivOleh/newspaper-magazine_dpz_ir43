using System.ComponentModel.DataAnnotations;
using CURSOVA.Application.Advertising;

namespace CURSOVA.Areas.Admin.Models;

public class PartnerForm
{
    public int? Id { get; set; }

    [Required(ErrorMessage = "Вкажіть назву.")]
    [MaxLength(100)]
    [Display(Name = "Назва бренду")]
    public string Name { get; set; } = "";

    [MaxLength(200)]
    [Display(Name = "Слоган")]
    public string? Tagline { get; set; }

    [Required(ErrorMessage = "Вкажіть сайт.")]
    [MaxLength(500)]
    [Display(Name = "Сайт")]
    public string WebsiteUrl { get; set; } = "";

    [MaxLength(500)]
    [Display(Name = "Логотип (URL)")]
    public string? LogoUrl { get; set; }

    [Display(Name = "Активний")]
    public bool IsActive { get; set; } = true;

    public static PartnerForm From(PartnerDto p) => new()
    {
        Id = p.Id, Name = p.Name, Tagline = p.Tagline, WebsiteUrl = p.WebsiteUrl, LogoUrl = p.LogoUrl, IsActive = p.IsActive
    };

    public SavePartnerRequest ToRequest() => new(Name, WebsiteUrl, Tagline, LogoUrl, IsActive);
}

public class AdForm
{
    public int? Id { get; set; }

    [Required(ErrorMessage = "Вкажіть заголовок.")]
    [MaxLength(120)]
    [Display(Name = "Заголовок")]
    public string Headline { get; set; } = "";

    [MaxLength(300)]
    [Display(Name = "Текст")]
    public string? Body { get; set; }

    [MaxLength(500)]
    [Display(Name = "Зображення (URL)")]
    public string? ImageUrl { get; set; }

    [Required(ErrorMessage = "Вкажіть посилання.")]
    [MaxLength(500)]
    [Display(Name = "Посилання (https://… або /шлях)")]
    public string TargetUrl { get; set; } = "";

    [Required(ErrorMessage = "Вкажіть текст кнопки.")]
    [MaxLength(40)]
    [Display(Name = "Текст кнопки")]
    public string CallToAction { get; set; } = "Детальніше";

    [Display(Name = "Партнер")]
    public int? PartnerId { get; set; }

    [Display(Name = "Показувати в рубриці")]
    public int? CategoryId { get; set; }

    [Display(Name = "Активне")]
    public bool IsActive { get; set; } = true;

    public static AdForm From(AdDto a) => new()
    {
        Id = a.Id, Headline = a.Headline, Body = a.Body, ImageUrl = a.ImageUrl, TargetUrl = a.TargetUrl,
        CallToAction = a.CallToAction, PartnerId = a.PartnerId, CategoryId = a.CategoryId, IsActive = a.IsActive
    };

    public SaveAdRequest ToRequest() => new(Headline, TargetUrl, CallToAction, Body, ImageUrl, PartnerId, CategoryId, IsActive);
}
