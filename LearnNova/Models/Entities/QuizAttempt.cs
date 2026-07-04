namespace LearnNova.Models.Entities;

public class QuizAttempt
{
    public int Id { get; set; }
    public int Score { get; set; }
    public int TotalPoints { get; set; }
    public double Percentage { get; set; }
    public bool Passed { get; set; }
    public string AnswersJson { get; set; } = "{}";
    public int AttemptNumber { get; set; }
    public int DurationSeconds { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

    public int QuizId { get; set; }
    public Quiz Quiz { get; set; } = null!;

    public string StudentId { get; set; } = string.Empty;
    public ApplicationUser Student { get; set; } = null!;
}
