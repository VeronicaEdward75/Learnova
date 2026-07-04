namespace LearnNova.Models.ViewModels.Teacher;

public class QuizAnalyticsViewModel
{
    public int QuizId { get; set; }
    public string QuizTitle { get; set; } = string.Empty;
    public int CourseId { get; set; }
    
    // Aggregates
    public int TotalAttempts { get; set; }
    public int UniqueStudentsAttempted { get; set; }
    public int HighestScore { get; set; }
    public int LowestScore { get; set; }
    public double AverageScore { get; set; }
    public double PassRate { get; set; }
    public double FailRate { get; set; }
    
    // Search & Sort state
    public string SearchTerm { get; set; } = string.Empty;
    public string SortBy { get; set; } = string.Empty;
    
    // Student Table Data
    public List<QuizAttemptSummaryDto> StudentAttempts { get; set; } = new();
}

public class QuizAttemptSummaryDto
{
    public int AttemptId { get; set; }
    public string StudentId { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public int Score { get; set; }
    public int TotalPoints { get; set; }
    public double Percentage { get; set; }
    public bool Passed { get; set; }
    public int AttemptNumber { get; set; }
    public DateTime SubmittedAt { get; set; }
}
