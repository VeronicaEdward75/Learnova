using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LearnNova.Models.Entities;
using LearnNova.Repositories;

namespace LearnNova.Services;

public class CertificateService : ICertificateService
{
    private readonly IGenericRepository<Certificate> _certRepo;
    private readonly IGenericRepository<ContentProgress> _progressRepo;
    private readonly IGenericRepository<CourseContent> _contentRepo;
    private readonly ICourseRepository _courseRepo;
    private readonly IDocumentService _documentService;
    private readonly IEmailService _emailService;
    private readonly IGenericRepository<Quiz> _quizRepo;
    private readonly IGenericRepository<QuizAttempt> _quizAttemptRepo;
    private readonly IGenericRepository<Assignment> _assignmentRepo;
    private readonly IGenericRepository<AssignmentSubmission> _submissionRepo;

    public CertificateService(
        IGenericRepository<Certificate> certRepo,
        IGenericRepository<ContentProgress> progressRepo,
        IGenericRepository<CourseContent> contentRepo,
        ICourseRepository courseRepo,
        IDocumentService documentService,
        IEmailService emailService,
        IGenericRepository<Quiz> quizRepo,
        IGenericRepository<QuizAttempt> quizAttemptRepo,
        IGenericRepository<Assignment> assignmentRepo,
        IGenericRepository<AssignmentSubmission> submissionRepo)
    {
        _certRepo = certRepo;
        _progressRepo = progressRepo;
        _contentRepo = contentRepo;
        _courseRepo = courseRepo;
        _documentService = documentService;
        _emailService = emailService;
        _quizRepo = quizRepo;
        _quizAttemptRepo = quizAttemptRepo;
        _assignmentRepo = assignmentRepo;
        _submissionRepo = submissionRepo;
    }

    public async Task<(bool Success, string Message, Certificate? Certificate)> CheckAndIssueCertificateAsync(string studentId, int courseId)
    {
        // 1. Check if certificate already exists
        var existing = (await _certRepo.FindAsync(c => c.StudentId == studentId && c.CourseId == courseId)).FirstOrDefault();
        if (existing != null) return (true, "Certificate already issued", existing);

        // 2. Calculate completion
        var missingRequirements = new List<string>();

        // Lessons
        var totalContents = (await _contentRepo.FindAsync(c => c.CourseId == courseId)).Count();
        var completedContents = (await _progressRepo.FindAsync(p => p.StudentId == studentId && p.Content.CourseId == courseId && p.IsCompleted)).Count();
        if (completedContents < totalContents)
        {
            missingRequirements.Add($"Lessons ({completedContents}/{totalContents})");
        }

        // Quizzes
        var quizzes = await _quizRepo.FindAsync(q => q.CourseId == courseId && q.IsPublished);
        var totalQuizzes = quizzes.Count();
        var passedQuizzes = 0;
        foreach (var q in quizzes)
        {
            var passed = (await _quizAttemptRepo.FindAsync(a => a.QuizId == q.Id && a.StudentId == studentId && a.Passed)).Any();
            if (passed) passedQuizzes++;
        }
        if (passedQuizzes < totalQuizzes)
        {
            missingRequirements.Add($"Quizzes ({passedQuizzes}/{totalQuizzes})");
        }

        // Assignments
        var assignments = await _assignmentRepo.FindAsync(a => a.CourseId == courseId);
        var totalAssignments = assignments.Count();
        var submittedAssignments = 0;
        foreach (var a in assignments)
        {
            var submitted = (await _submissionRepo.FindAsync(s => s.AssignmentId == a.Id && s.StudentId == studentId)).Any();
            if (submitted) submittedAssignments++;
        }
        if (submittedAssignments < totalAssignments)
        {
            missingRequirements.Add($"Assignments ({submittedAssignments}/{totalAssignments})");
        }

        if (missingRequirements.Any())
        {
            return (false, string.Join(", ", missingRequirements), null);
        }

        if (totalContents == 0 && totalQuizzes == 0 && totalAssignments == 0)
        {
            return (false, "Course has no requirements", null);
        }

        // 3. Issue certificate
        var course = await _courseRepo.GetByIdAsync(courseId);
        var cert = new Certificate
        {
            StudentId = studentId,
            CourseId = courseId,
            IssueDate = DateTime.UtcNow,
            VerificationCode = Guid.NewGuid().ToString("N").Substring(0, 12).ToUpper(),
            CertificateNumber = $"LN-{DateTime.UtcNow.Year}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}"
        };

        await _certRepo.AddAsync(cert);
        await _certRepo.SaveChangesAsync();

        // Optional: Generate PDF and send email
        return (true, "Certificate issued successfully", cert);
    }

    public async Task<Certificate?> GetCertificateByCodeAsync(string verificationCode)
    {
        var certs = await _certRepo.FindAsync(c => c.VerificationCode == verificationCode || c.CertificateNumber == verificationCode);
        var cert = certs.FirstOrDefault();
        if (cert != null)
        {
            cert.Course = (await _courseRepo.GetByIdAsync(cert.CourseId))!;
        }
        return cert;
    }

    public async Task<IEnumerable<Certificate>> GetStudentCertificatesAsync(string studentId)
    {
        return await _certRepo.GetQueryable().Include(c => c.Course).ThenInclude(c => c.Teacher).Where(c => c.StudentId == studentId).ToListAsync();
    }
}
