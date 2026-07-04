namespace LearnNova.Models.ViewModels.Student;

public class StudentQuizHistoryViewModel
{
    public int QuizId { get; set; }
    public string QuizTitle { get; set; } = string.Empty;
    public int CourseId { get; set; }
    public bool ShowAnswers { get; set; }
    
    // Ordered newest first
    public List<StudentAttemptDetailsDto> Attempts { get; set; } = new();
}

public class StudentAttemptDetailsDto
{
    public int AttemptId { get; set; }
    public int AttemptNumber { get; set; }
    public int Score { get; set; }
    public int TotalPoints { get; set; }
    public double Percentage { get; set; }
    public bool Passed { get; set; }
    public int DurationSeconds { get; set; }
    public DateTime SubmittedAt { get; set; }
    
    // QuestionId -> Tuple<QuestionText, StudentAnswer, CorrectAnswer, Points, MaxPoints>
    public List<StudentAnswerBreakdownDto> AnswersBreakdown { get; set; } = new();
}

public class StudentAnswerBreakdownDto
{
    public int QuestionId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public string StudentAnswer { get; set; } = string.Empty;
    public string CorrectAnswer { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
    public int Points { get; set; }
    public int MaxPoints { get; set; }
}
