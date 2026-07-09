using System;
using System.ComponentModel.DataAnnotations;
using LearnNova.Models.Enums;

namespace LearnNova.Models.ViewModels.Teacher;

public class TeacherCouponFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "كود الخصم مطلوب")]
    [Display(Name = "كود الخصم")]
    public string Code { get; set; } = string.Empty;

    [Display(Name = "نوع الخصم")]
    public DiscountType DiscountType { get; set; }

    [Display(Name = "قيمة الخصم")]
    public decimal DiscountValue { get; set; }

    [Display(Name = "تاريخ البدء")]
    public DateTime? StartDate { get; set; }

    [Display(Name = "تاريخ الانتهاء")]
    public DateTime? EndDate { get; set; }

    [Display(Name = "حد الاستخدام الأقصى")]
    public int? UsageLimit { get; set; }

    [Display(Name = "الكورس")]
    public int? CourseId { get; set; }

    [Display(Name = "مفعل")]
    public bool IsActive { get; set; } = true;
}
