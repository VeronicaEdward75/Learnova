namespace LearnNova.Models.Entities;

public class StudentQuestion
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? Answer { get; set; }
    public bool IsAnswered { get; set; }
    public DateTime? AnsweredAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string StudentId { get; set; } = string.Empty;
    public ApplicationUser Student { get; set; } = null!;

    public int CourseId { get; set; }
    public Course Course { get; set; } = null!;
}
