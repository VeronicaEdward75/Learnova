namespace LearnNova.Models.ViewModels.Student;

public class QuizResultViewModel
{
    public int QuizId { get; set; }
    public string QuizTitle { get; set; } = string.Empty;
    public int CourseId { get; set; }
    
    public int Score { get; set; }
    public int TotalPoints { get; set; }
    public double Percentage { get; set; }
    public bool Passed { get; set; }
    
    public int AttemptNumber { get; set; }
    public int MaxAttempts { get; set; }
    
    public int DurationSeconds { get; set; }
}
