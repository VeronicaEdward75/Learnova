namespace LearnNova.Models.Entities;

public class Assignment
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? FileUrl { get; set; }
    public DateTime DueDate { get; set; }
    public int MaxGrade { get; set; }

    public int CourseId { get; set; }
    public Course Course { get; set; } = null!;

    public ICollection<AssignmentSubmission> Submissions { get; set; } = new List<AssignmentSubmission>();
}
