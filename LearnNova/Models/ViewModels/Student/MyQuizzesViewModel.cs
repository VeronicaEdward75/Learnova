using LearnNova.Models.Entities;

namespace LearnNova.Models.ViewModels.Student;

public class MyQuizzesViewModel
{
    public int TotalQuizzes { get; set; }
    public int PassedCount { get; set; }
    public int FailedCount { get; set; }
    public double AverageScore { get; set; }
    
    public LearnNova.Models.ViewModels.PagedResult<MyQuizCardViewModel> Quizzes { get; set; } = new();
}

public class MyQuizCardViewModel
{
    public int QuizId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public string InstructorName { get; set; } = string.Empty;
    public string QuizTitle { get; set; } = string.Empty;
    public int TimeLimitMinutes { get; set; }
    public int QuestionCount { get; set; }
    public int PassingScore { get; set; }
    public DateTime CreatedAt { get; set; }
    
    public int MaxAttempts { get; set; }
    public int TotalAttemptsMade { get; set; }
    public int CompletedAttempts { get; set; }
    public int RemainingAttempts => System.Math.Max(0, MaxAttempts - TotalAttemptsMade);
    
    public int BestScore { get; set; }
    public int LatestScore { get; set; }
    
    public bool HasUncompletedAttempt { get; set; }
    public bool Passed { get; set; }
    
    public string Status 
    {
        get 
        {
            if (TotalAttemptsMade == 0) return "Not Started";
            if (HasUncompletedAttempt) return "In Progress";
            if (Passed) return "Passed";
            return "Failed";
        }
    }
}
