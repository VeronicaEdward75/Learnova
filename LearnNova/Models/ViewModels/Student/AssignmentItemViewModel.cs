using LearnNova.Models.Entities;

namespace LearnNova.Models.ViewModels.Student;

public class AssignmentItemViewModel
{
    public Assignment Assignment { get; set; } = null!;
    public AssignmentSubmission? Submission { get; set; }
}
