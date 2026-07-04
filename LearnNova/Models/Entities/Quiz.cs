namespace LearnNova.Models.Entities;

public class Quiz
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int TimeLimitMinutes { get; set; }
    public int PassingScore { get; set; } = 50;
    public int MaxAttempts { get; set; } = 1;
    public bool IsPublished { get; set; }
    public bool ShowAnswers { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int CourseId { get; set; }
    public Course Course { get; set; } = null!;

    public ICollection<QuizQuestion> Questions { get; set; } = new List<QuizQuestion>();
    public ICollection<QuizAttempt> Attempts { get; set; } = new List<QuizAttempt>();
}
