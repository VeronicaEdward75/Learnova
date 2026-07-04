using System.ComponentModel.DataAnnotations;
using LearnNova.Models.Enums;

namespace LearnNova.Models.ViewModels.Teacher;

public class QuestionFormViewModel : IValidatableObject
{
    public int Id { get; set; }
    public int QuizId { get; set; }
    public int OrderIndex { get; set; }

    [Required(ErrorMessage = "نص السؤال مطلوب")]
    [Display(Name = "السؤال")]
    public string Text { get; set; } = string.Empty;

    [Required(ErrorMessage = "نوع السؤال مطلوب")]
    [Display(Name = "النوع")]
    public QuestionType Type { get; set; }

    [Display(Name = "الخيار أ / صح")]
    public string OptionA { get; set; } = string.Empty;

    [Display(Name = "الخيار ب / خطأ")]
    public string OptionB { get; set; } = string.Empty;

    [Display(Name = "الخيار ج")]
    public string? OptionC { get; set; }

    [Display(Name = "الخيار د")]
    public string? OptionD { get; set; }

    [Required(ErrorMessage = "الإجابة الصحيحة مطلوبة")]
    [Display(Name = "الإجابة الصحيحة (أ، ب، ج، د) أو (صح، خطأ)")]
    public string CorrectAnswer { get; set; } = string.Empty;

    [Required(ErrorMessage = "عدد النقاط مطلوب")]
    [Range(1, 100, ErrorMessage = "النقاط يجب أن تكون بين 1 و 100")]
    [Display(Name = "النقاط")]
    public int Points { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Type == QuestionType.MultipleChoice)
        {
            if (string.IsNullOrWhiteSpace(OptionA))
                yield return new ValidationResult("الخيار أ مطلوب في أسئلة الاختيار من متعدد", new[] { nameof(OptionA) });
            
            if (string.IsNullOrWhiteSpace(OptionB))
                yield return new ValidationResult("الخيار ب مطلوب في أسئلة الاختيار من متعدد", new[] { nameof(OptionB) });
        }
    }
}
