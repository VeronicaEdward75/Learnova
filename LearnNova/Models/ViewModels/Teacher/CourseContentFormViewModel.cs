using System.ComponentModel.DataAnnotations;
using LearnNova.Models.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LearnNova.Models.ViewModels.Teacher;

public class CourseContentFormViewModel : IValidatableObject
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "يرجى إدخال عنوان المحتوى")]
    [Display(Name = "العنوان")]
    public string Title { get; set; } = string.Empty;
    
    [Display(Name = "نوع المصدر")]
    public string SourceType { get; set; } = "URL";

    [Display(Name = "الرابط (فيديو أو ملف خارجي)")]
    public string? FileUrl { get; set; }

    [Display(Name = "رفع ملف")]
    public IFormFile? UploadFile { get; set; }
    
    [Display(Name = "المدة (بالثواني)")]
    public int? DurationSec { get; set; }
    
    public int CourseId { get; set; }
    
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (SourceType == "URL" && string.IsNullOrWhiteSpace(FileUrl))
        {
            yield return new ValidationResult("يرجى إدخال الرابط الخارجي.", new[] { nameof(FileUrl) });
        }
        else if (SourceType == "UPLOAD" && UploadFile == null && Id == 0)
        {
            yield return new ValidationResult("يرجى رفع الملف.", new[] { nameof(UploadFile) });
        }
    }
}
