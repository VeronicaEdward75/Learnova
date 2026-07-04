using System.ComponentModel.DataAnnotations;
using LearnNova.Models.Lookups;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LearnNova.Models.ViewModels.Teacher;

/// <summary>
/// Shared ViewModel used for both Create and Edit course forms.
/// </summary>
public class CourseFormViewModel
{
    [Required(ErrorMessage = "عنوان الكورس مطلوب")]
    [StringLength(200, ErrorMessage = "العنوان لا يتجاوز 200 حرف")]
    [Display(Name = "عنوان الكورس")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "الوصف مطلوب")]
    [Display(Name = "الوصف")]
    [StringLength(2000)]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "المادة مطلوبة")]
    [Display(Name = "المادة")]
    public string Subject { get; set; } = string.Empty;

    [Required(ErrorMessage = "المرحلة الدراسية مطلوبة")]
    [Display(Name = "المرحلة")]
    public string Stage { get; set; } = string.Empty;

    [Required(ErrorMessage = "الصف الدراسي مطلوب")]
    [Display(Name = "الصف الدراسي")]
    public int GradeLevel { get; set; }

    [Required(ErrorMessage = "السعر مطلوب")]
    [Range(0, 100000, ErrorMessage = "السعر يجب أن يكون بين 0 و 100000")]
    [Display(Name = "السعر (جنيه)")]
    public decimal Price { get; set; }

    // ─── Select list sources (populated by controller before returning view) ───
    public List<SelectListItem> SubjectOptions { get; set; } = BuildSubjectList();
    public List<SelectListItem> StageOptions   { get; set; } = BuildStageList();
    public List<SelectListItem> GradeOptions   { get; set; } = new();

    // ─── Helpers ─────────────────────────────────────────────────────────────
    private static List<SelectListItem> BuildSubjectList() =>
        CurriculumLookup.Subjects
            .Select(kvp => new SelectListItem(kvp.Value, kvp.Key))
            .ToList();

    private static List<SelectListItem> BuildStageList() =>
        CurriculumLookup.Stages
            .Select(s => new SelectListItem(s, s))
            .ToList();

    public static List<SelectListItem> BuildGradeList(string stage)
    {
        if (!CurriculumLookup.StageGrades.TryGetValue(stage, out var grades))
            return new();

        return grades.Select(g => new SelectListItem(g.Label, g.Num.ToString())).ToList();
    }
}
