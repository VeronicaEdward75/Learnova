using System.Threading.Tasks;

namespace LearnNova.Services;

public interface IEmailService
{
    Task SendEmailAsync(string toEmail, string subject, string body);
    Task SendEmailWithAttachmentAsync(string toEmail, string subject, string body, string attachmentName, byte[] attachmentData);
}
