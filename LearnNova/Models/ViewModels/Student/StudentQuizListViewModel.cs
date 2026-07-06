namespace LearnNova.Models.ViewModels.Student;

public class StudentQuizListViewModel
{
    public int CourseId { get; set; }
    public List<StudentQuizCardViewModel> Quizzes { get; set; } = new();
}

public class StudentQuizCardViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int TimeLimitMinutes { get; set; }
    public int PassingScore { get; set; }
    public int QuestionCount { get; set; }
    public int MaxAttempts { get; set; }
    public int UsedAttempts { get; set; }
    public int RemainingAttempts => System.Math.Max(0, MaxAttempts - UsedAttempts);
}
