using System.ComponentModel.DataAnnotations;

namespace LearnNova.Models.ViewModels.Account;

public class ResetPasswordViewModel
{
    [Required]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Token { get; set; } = string.Empty;

    [Required(ErrorMessage = "كلمة المرور مطلوبة")]
    [StringLength(100, ErrorMessage = "كلمة المرور يجب أن تكون {2} أحرف على الأقل.", MinimumLength = 8)]
    [DataType(DataType.Password)]
    [Display(Name = "كلمة المرور الجديدة")]
    public string Password { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "تأكيد كلمة المرور")]
    [Compare("Password", ErrorMessage = "كلمة المرور غير متطابقة")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
