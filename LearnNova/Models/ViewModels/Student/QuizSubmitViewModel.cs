using System.ComponentModel.DataAnnotations;

namespace LearnNova.Models.ViewModels.Student;

public class QuizSubmitViewModel
{
    public int QuizId { get; set; }
    
    [Required]
    public int AttemptId { get; set; }

    // Dictionary mapping QuestionId -> Selected Answer (e.g., "أ", "ب", "صح", "خطأ")
    public Dictionary<int, string> Answers { get; set; } = new();
}
