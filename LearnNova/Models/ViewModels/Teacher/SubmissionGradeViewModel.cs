using System.ComponentModel.DataAnnotations;

namespace LearnNova.Models.ViewModels.Teacher;

public class SubmissionGradeViewModel
{
    public int SubmissionId { get; set; }
    public int AssignmentId { get; set; }

    [Required(ErrorMessage = "يرجى إدخال الدرجة")]
    public int Grade { get; set; }

    public string? TeacherComment { get; set; }
}
