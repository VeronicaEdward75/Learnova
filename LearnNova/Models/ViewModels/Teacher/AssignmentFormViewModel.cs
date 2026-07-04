using System.ComponentModel.DataAnnotations;

namespace LearnNova.Models.ViewModels.Teacher;

public class AssignmentFormViewModel
{
    public int Id { get; set; }

    public int CourseId { get; set; }

    [Required(ErrorMessage = "عنوان المهمة مطلوب")]
    [StringLength(200, ErrorMessage = "العنوان لا يتجاوز 200 حرف")]
    [Display(Name = "العنوان")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "وصف المهمة مطلوب")]
    [Display(Name = "الوصف")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "تاريخ الاستحقاق مطلوب")]
    [Display(Name = "تاريخ الاستحقاق")]
    [DataType(DataType.DateTime)]
    public DateTime DueDate { get; set; }

    [Required(ErrorMessage = "الدرجة القصوى مطلوبة")]
    [Range(1, 10000, ErrorMessage = "الدرجة القصوى يجب أن تكون أكبر من 0")]
    [Display(Name = "الدرجة القصوى")]
    public int MaxGrade { get; set; } = 100;
}
