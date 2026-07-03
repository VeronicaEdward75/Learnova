using LearnNova.Models.Enums;
using Microsoft.AspNetCore.Identity;

namespace LearnNova.Models.Entities;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Student-only fields
    public string? Stage { get; set; }
    public int? GradeLevel { get; set; }
    public string? GradeLabel { get; set; }
    public string? Section { get; set; }

    // Teacher-only fields
    public string? Subject { get; set; }
    public string? TeachStage { get; set; }

    public ICollection<Course> CoursesTaught { get; set; } = new List<Course>();
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    public ICollection<ContentProgress> ContentProgressRecords { get; set; } = new List<ContentProgress>();
    public ICollection<QuizAttempt> QuizAttempts { get; set; } = new List<QuizAttempt>();
    public ICollection<AssignmentSubmission> AssignmentSubmissions { get; set; } = new List<AssignmentSubmission>();
    public ICollection<StudentQuestion> Questions { get; set; } = new List<StudentQuestion>();
    public ICollection<ChatMessage> SentMessages { get; set; } = new List<ChatMessage>();
    public ICollection<ChatMessage> ReceivedMessages { get; set; } = new List<ChatMessage>();
}
