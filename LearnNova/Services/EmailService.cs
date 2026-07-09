using System.Net;
using System.Net.Mail;
using System.IO;
using LearnNova.Models.Settings;
using Microsoft.Extensions.Options;

namespace LearnNova.Services;

public class EmailService : IEmailService
{
    private readonly SmtpSettings _smtpSettings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<SmtpSettings> smtpSettings, ILogger<EmailService> logger)
    {
        _smtpSettings = smtpSettings.Value;
        _logger = logger;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        try
        {
            _logger.LogInformation("Sending email to {ToEmail} via {Host}:{Port}...", toEmail, _smtpSettings.Host, _smtpSettings.Port);

            using var client = new SmtpClient(_smtpSettings.Host, _smtpSettings.Port)
            {
                Credentials = new NetworkCredential(_smtpSettings.Username, _smtpSettings.Password),
                EnableSsl = _smtpSettings.EnableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_smtpSettings.FromEmail, _smtpSettings.FromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mailMessage.To.Add(toEmail);

            await client.SendMailAsync(mailMessage);
            _logger.LogInformation("Email sent successfully to {ToEmail}.", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {ToEmail}. SMTP Host: {Host}. Error: {Error}", toEmail, _smtpSettings.Host, ex.Message);
            throw; // Do NOT swallow the exception so the app can handle or expose it appropriately
        }
    }

    public async Task SendEmailWithAttachmentAsync(string toEmail, string subject, string body, string attachmentName, byte[] attachmentData)
    {
        try
        {
            _logger.LogInformation("Sending email with attachment {AttachmentName} to {ToEmail}...", attachmentName, toEmail);

            using var client = new SmtpClient(_smtpSettings.Host, _smtpSettings.Port)
            {
                Credentials = new NetworkCredential(_smtpSettings.Username, _smtpSettings.Password),
                EnableSsl = _smtpSettings.EnableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_smtpSettings.FromEmail, _smtpSettings.FromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mailMessage.To.Add(toEmail);

            using var ms = new MemoryStream(attachmentData);
            var attachment = new Attachment(ms, attachmentName, "application/pdf");
            mailMessage.Attachments.Add(attachment);

            await client.SendMailAsync(mailMessage);
            _logger.LogInformation("Email with attachment sent successfully to {ToEmail}.", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email with attachment to {ToEmail}. Error: {Error}", toEmail, ex.Message);
            throw;
        }
    }
}
