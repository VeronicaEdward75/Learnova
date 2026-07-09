using System.ComponentModel.DataAnnotations;
using System;

namespace LearnNova.Models.ViewModels.Student;

public class SecurityViewModel
{
    [Required(ErrorMessage = "كلمة المرور الحالية مطلوبة")]
    [DataType(DataType.Password)]
    [Display(Name = "كلمة المرور الحالية")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "كلمة المرور الجديدة مطلوبة")]
    [StringLength(100, ErrorMessage = "كلمة المرور يجب أن تكون {2} أحرف على الأقل.", MinimumLength = 8)]
    [DataType(DataType.Password)]
    [Display(Name = "كلمة المرور الجديدة")]
    public string NewPassword { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "تأكيد كلمة المرور")]
    [Compare("NewPassword", ErrorMessage = "كلمة المرور الجديدة وتأكيدها لا يتطابقان.")]
    public string ConfirmPassword { get; set; } = string.Empty;
    
    public DateTime? LastLoginDate { get; set; }
}
