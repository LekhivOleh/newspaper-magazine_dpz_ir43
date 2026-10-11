using System.ComponentModel.DataAnnotations;

namespace CURSOVA.Models;

public class LoginViewModel
{
    [Required(ErrorMessage = "Вкажіть email.")]
    [EmailAddress(ErrorMessage = "Некоректний email.")]
    [Display(Name = "Email")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Вкажіть пароль.")]
    [DataType(DataType.Password)]
    [Display(Name = "Пароль")]
    public string Password { get; set; } = "";

    [Display(Name = "Запам'ятати мене")]
    public bool RememberMe { get; set; }

    public string? ReturnUrl { get; set; }
}

public class RegisterViewModel
{
    [Required(ErrorMessage = "Вкажіть ім'я.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Ім'я має містити від 2 до 100 символів.")]
    [Display(Name = "Ім'я")]
    public string DisplayName { get; set; } = "";

    [Required(ErrorMessage = "Вкажіть email.")]
    [EmailAddress(ErrorMessage = "Некоректний email.")]
    [Display(Name = "Email")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Вкажіть пароль.")]
    [DataType(DataType.Password)]
    [Display(Name = "Пароль")]
    public string Password { get; set; } = "";

    [Required(ErrorMessage = "Повторіть пароль.")]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Паролі не збігаються.")]
    [Display(Name = "Повторіть пароль")]
    public string ConfirmPassword { get; set; } = "";

    public string? ReturnUrl { get; set; }
}
