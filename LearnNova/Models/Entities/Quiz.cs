namespace LearnNova.Models.Entities;

public class Quiz
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int DurationMin { get; set; }
    public DateTime? DueDate { get; set; }
    public bool IsPublished { get; set; }

    public int CourseId { get; set; }
    public Course Course { get; set; } = null!;

    public ICollection<QuizQuestion> Questions { get; set; } = new List<QuizQuestion>();
    public ICollection<QuizAttempt> Attempts { get; set; } = new List<QuizAttempt>();
}
