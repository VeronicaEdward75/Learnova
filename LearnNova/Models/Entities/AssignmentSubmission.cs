using LearnNova.Models.Enums;

namespace LearnNova.Models.Entities;

public class AssignmentSubmission
{
    public int Id { get; set; }
    public string? FileUrl { get; set; }
    public string? TextAnswer { get; set; }
    public int? Grade { get; set; }
    public SubmitStatus Status { get; set; } = SubmitStatus.Submitted;
    public string? TeacherComment { get; set; }
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

    public int AssignmentId { get; set; }
    public Assignment Assignment { get; set; } = null!;

    public string StudentId { get; set; } = string.Empty;
    public ApplicationUser Student { get; set; } = null!;
}
