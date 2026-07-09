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

    public CertificateService(
        IGenericRepository<Certificate> certRepo,
        IGenericRepository<ContentProgress> progressRepo,
        IGenericRepository<CourseContent> contentRepo,
        ICourseRepository courseRepo,
        IDocumentService documentService,
        IEmailService emailService)
    {
        _certRepo = certRepo;
        _progressRepo = progressRepo;
        _contentRepo = contentRepo;
        _courseRepo = courseRepo;
        _documentService = documentService;
        _emailService = emailService;
    }

    public async Task<(bool Success, string Message, Certificate? Certificate)> CheckAndIssueCertificateAsync(string studentId, int courseId)
    {
        // 1. Check if certificate already exists
        var existing = (await _certRepo.FindAsync(c => c.StudentId == studentId && c.CourseId == courseId)).FirstOrDefault();
        if (existing != null) return (true, "Certificate already issued", existing);

        // 2. Calculate completion
        var totalContents = (await _contentRepo.FindAsync(c => c.CourseId == courseId)).Count();
        if (totalContents == 0) return (false, "Course has no content", null);

        // A content is completed if it's in ContentProgress with IsCompleted = true
        var completedContents = (await _progressRepo.FindAsync(p => p.StudentId == studentId && p.Content.CourseId == courseId && p.IsCompleted)).Count();

        if (completedContents < totalContents)
        {
            return (false, $"Course is not 100% completed ({completedContents}/{totalContents})", null);
        }

        // 3. Issue certificate
        var course = await _courseRepo.GetByIdAsync(courseId);
        var cert = new Certificate
        {
            StudentId = studentId,
            CourseId = courseId,
            CertificateNumber = $"LN-{DateTime.UtcNow.Year}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}",
            VerificationCode = Guid.NewGuid().ToString().ToUpper(),
            IssueDate = DateTime.UtcNow,
            Grade = 100.0
        };

        await _certRepo.AddAsync(cert);
        await _certRepo.SaveChangesAsync();

        // 4. Send Email with PDF
        try
        {
            var pdfData = await _documentService.GenerateCertificatePdfAsync(cert.Id);
            var student = course?.Enrollments.FirstOrDefault(e => e.StudentId == studentId)?.Student; // Assuming we can get email. If not, inject UserManager.
            if (student != null)
            {
                await _emailService.SendEmailWithAttachmentAsync(
                    student.Email!,
                    "Congratulations! Here is your Certificate of Completion",
                    $"Hi {student.FullName},<br>You have successfully completed {course?.Title}. Please find your certificate attached.",
                    $"Certificate_{cert.CertificateNumber}.pdf",
                    pdfData);
            }
        }
        catch { /* Log failure but don't prevent certificate issuance */ }

        return (true, "Certificate issued successfully", cert);
    }

    public async Task<Certificate?> GetCertificateByCodeAsync(string verificationCode)
    {
        var certs = await _certRepo.FindAsync(c => c.VerificationCode == verificationCode || c.CertificateNumber == verificationCode);
        return certs.FirstOrDefault();
    }

    public async Task<IEnumerable<Certificate>> GetStudentCertificatesAsync(string studentId)
    {
        return await _certRepo.GetQueryable().Include(c => c.Course).ThenInclude(c => c.Teacher).Where(c => c.StudentId == studentId).ToListAsync();
    }
}

