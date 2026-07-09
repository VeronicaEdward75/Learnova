using System;

namespace LearnNova.Models.Entities;

public class Certificate
{
    public int Id { get; set; }
    
    public string CertificateNumber { get; set; } = string.Empty;
    public string VerificationCode { get; set; } = string.Empty;
    public DateTime IssueDate { get; set; } = DateTime.UtcNow;
    public double Grade { get; set; } = 100.0;

    public string StudentId { get; set; } = string.Empty;
    public ApplicationUser Student { get; set; } = null!;

    public int CourseId { get; set; }
    public Course Course { get; set; } = null!;
}
