using LearnNova.Models.Entities;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace LearnNova.Models.ViewModels.Student;

public class AssignmentSubmitViewModel : IValidatableObject
{
    public Assignment Assignment { get; set; } = null!;
    public AssignmentSubmission? ExistingSubmission { get; set; }

    [Display(Name = "إجابة نصية (اختياري)")]
    public string? TextAnswer { get; set; }

    [Display(Name = "ملف مرفق (اختياري)")]
    public IFormFile? File { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (File == null && string.IsNullOrWhiteSpace(TextAnswer))
        {
            yield return new ValidationResult("يجب إرفاق ملف أو كتابة إجابة نصية.", new[] { nameof(File), nameof(TextAnswer) });
        }
    }
}
