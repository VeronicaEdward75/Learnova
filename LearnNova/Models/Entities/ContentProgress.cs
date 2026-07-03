namespace LearnNova.Models.Entities;

public class ContentProgress
{
    public int Id { get; set; }
    public bool IsCompleted { get; set; }
    public int? WatchedSeconds { get; set; }

    public string StudentId { get; set; } = string.Empty;
    public ApplicationUser Student { get; set; } = null!;

    public int ContentId { get; set; }
    public CourseContent Content { get; set; } = null!;
}
