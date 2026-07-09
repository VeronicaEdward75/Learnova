using System.Collections.Generic;
using System.Threading.Tasks;
using LearnNova.Models.Entities;

namespace LearnNova.Services;

public interface ICertificateService
{
    Task<(bool Success, string Message, Certificate? Certificate)> CheckAndIssueCertificateAsync(string studentId, int courseId);
    Task<Certificate?> GetCertificateByCodeAsync(string verificationCode);
    Task<IEnumerable<Certificate>> GetStudentCertificatesAsync(string studentId);
}
