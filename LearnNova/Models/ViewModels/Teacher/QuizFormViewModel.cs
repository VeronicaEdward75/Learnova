using System.ComponentModel.DataAnnotations;

namespace LearnNova.Models.ViewModels.Teacher;

public class QuizFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "عنوان الاختبار مطلوب")]
    [MaxLength(200, ErrorMessage = "العنوان يجب ألا يتجاوز 200 حرف")]
    [Display(Name = "العنوان")]
    public string Title { get; set; } = string.Empty;

    [Display(Name = "تم النشر")]
    public bool IsPublished { get; set; }

    [Display(Name = "إظهار الإجابات الصحيحة بعد الاختبار")]
    public bool ShowAnswers { get; set; }

    [Required(ErrorMessage = "وصف الاختبار مطلوب")]
    [Display(Name = "الوصف")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "وقت الاختبار مطلوب")]
    [Range(1, 1000, ErrorMessage = "وقت الاختبار يجب أن يكون أكبر من 0")]
    [Display(Name = "المدة الزمنية (بالدقائق)")]
    public int TimeLimitMinutes { get; set; }

    [Required(ErrorMessage = "درجة النجاح مطلوبة")]
    [Range(0, 1000, ErrorMessage = "درجة النجاح يجب أن تكون 0 أو أكثر")]
    [Display(Name = "درجة النجاح")]
    public int PassingScore { get; set; }

    [Required(ErrorMessage = "عدد المحاولات المسموح بها مطلوب")]
    [Range(1, 100, ErrorMessage = "عدد المحاولات يجب أن يكون 1 على الأقل")]
    [Display(Name = "أقصى عدد للمحاولات")]
    public int MaxAttempts { get; set; }

    public int CourseId { get; set; }
}
