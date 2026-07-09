using System;
using System.ComponentModel.DataAnnotations;
using LearnNova.Models.Enums;

namespace LearnNova.Models.ViewModels.Admin;

public class CouponFormViewModel
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

    [Display(Name = "الحد الأدنى للطلب")]
    public decimal? MinimumOrder { get; set; }

    [Display(Name = "الحد الأقصى للخصم (لنسبة الخصم)")]
    public decimal? MaximumDiscount { get; set; }

    [Display(Name = "حد الاستخدام الأقصى")]
    public int? UsageLimit { get; set; }

    [Display(Name = "مخصص لكورس محدد")]
    public int? CourseId { get; set; }

    [Display(Name = "مخصص لمدرس محدد")]
    public string? TeacherId { get; set; }

    [Display(Name = "مفعل")]
    public bool IsActive { get; set; } = true;
}

public class CouponListViewModel
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public DiscountType DiscountType { get; set; }
    public decimal DiscountValue { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? UsageLimit { get; set; }
    public int UsageCount { get; set; }
    public string? CourseTitle { get; set; }
    public string? TeacherName { get; set; }
    public bool IsActive { get; set; }
    
    public bool IsExpired => EndDate.HasValue && EndDate.Value < DateTime.UtcNow;
    public bool IsUsageLimitReached => UsageLimit.HasValue && UsageCount >= UsageLimit.Value;
    public bool IsValid => IsActive && !IsExpired && !IsUsageLimitReached && (!StartDate.HasValue || StartDate.Value <= DateTime.UtcNow);
}
