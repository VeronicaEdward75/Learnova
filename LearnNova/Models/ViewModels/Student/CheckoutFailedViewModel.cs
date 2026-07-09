namespace LearnNova.Models.ViewModels.Student;

public class CheckoutFailedViewModel
{
    public int CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
}
