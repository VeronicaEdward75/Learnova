using System.ComponentModel.DataAnnotations;
using LearnNova.Models.Enums;

namespace LearnNova.Models.ViewModels;

// Role is restricted to Student/Teacher at the ViewModel level by defaulting to Student and by
// AuthService.RegisterAsync rejecting anything else server-side — a raw POST could otherwise try
// to submit Role=Admin, so the [Required] here is not the only line of defense.
public class RegisterViewModel
{
    [Required(ErrorMessage = "الاسم بالكامل مطلوب")]
    [Display(Name = "الاسم بالكامل")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
    [EmailAddress(ErrorMessage = "البريد الإلكتروني غير صحيح")]
    [Display(Name = "البريد الإلكتروني")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "كلمة المرور مطلوبة")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "كلمة المرور يجب ألا تقل عن 8 أحرف")]
    [DataType(DataType.Password)]
    [Display(Name = "كلمة المرور")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "تأكيد كلمة المرور مطلوب")]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "كلمتا المرور غير متطابقتين")]
    [Display(Name = "تأكيد كلمة المرور")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required]
    public UserRole Role { get; set; } = UserRole.Student;

    // Student-only
    public string? Stage { get; set; }
    public int? GradeNum { get; set; }

    // Teacher-only
    public string? Subject { get; set; }
    public string? TeachStage { get; set; }
}
