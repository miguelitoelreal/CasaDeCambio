using System.Net;
using System.Net.Mail;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MonitoringPlatform.API.Configurations;
using MonitoringPlatform.Application.Interfaces;
using MonitoringPlatform.Infrastructure.Persistence;

namespace MonitoringPlatform.API.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailOptions _fallbackOptions;
        private readonly AppDbContext _dbContext;
        private readonly ICurrentUserContext _currentUserContext;
        private readonly ILogger<EmailService> _logger;

        public EmailService(
            IOptions<EmailOptions> options,
            AppDbContext dbContext,
            ICurrentUserContext currentUserContext,
            ILogger<EmailService> logger)
        {
            _fallbackOptions = options.Value;
            _dbContext = dbContext;
            _currentUserContext = currentUserContext;
            _logger = logger;
        }

        public async Task<bool> SendEmailAsync(string toEmail, string subject, string htmlBody)
        {
            var tenantId = _currentUserContext.TenantId;
            var tenantConfig = await _dbContext.TenantSettings
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.TenantId == tenantId);

            _logger.LogInformation("EmailService: TenantId={TenantId}, TenantSettings found={Found}, EmailEnabled={EmailEnabled}", tenantId, tenantConfig != null, tenantConfig?.EmailEnabled);

            var enabled = tenantConfig?.EmailEnabled ?? _fallbackOptions.Enabled;

            if (!enabled)
            {
                _logger.LogWarning("Email service is disabled. Cannot send to {ToEmail}: {Subject}", toEmail, subject);
                return false;
            }

            var smtpHost = tenantConfig?.SmtpHost ?? _fallbackOptions.SmtpHost;
            var smtpPort = tenantConfig?.SmtpPort ?? _fallbackOptions.SmtpPort;
            var smtpUsername = tenantConfig?.SmtpUsername ?? _fallbackOptions.SmtpUsername;
            var smtpPassword = tenantConfig?.SmtpPassword ?? _fallbackOptions.SmtpPassword;
            var senderEmail = tenantConfig?.SenderEmail ?? _fallbackOptions.SenderEmail;
            var senderName = tenantConfig?.SenderName ?? _fallbackOptions.SenderName;
            var useSsl = tenantConfig?.UseSsl ?? _fallbackOptions.UseSsl;

            _logger.LogInformation("SMTP config: Host={Host}, Port={Port}, User={User}, Sender={Sender}, SSL={Ssl}", smtpHost, smtpPort, smtpUsername, senderEmail, useSsl);

            if (string.IsNullOrWhiteSpace(smtpHost) || string.IsNullOrWhiteSpace(smtpUsername) || string.IsNullOrWhiteSpace(smtpPassword))
            {
                _logger.LogWarning("SMTP config is incomplete. Host, Username or Password is empty.");
                return false;
            }

            try
            {
                using var client = new SmtpClient(smtpHost, smtpPort);
                client.EnableSsl = useSsl;
                client.Credentials = new NetworkCredential(smtpUsername, smtpPassword);

                var mail = new MailMessage
                {
                    From = new MailAddress(senderEmail, senderName),
                    Subject = subject,
                    Body = htmlBody,
                    IsBodyHtml = true,
                };
                mail.To.Add(toEmail);

                await client.SendMailAsync(mail);
                _logger.LogInformation("Email sent to {ToEmail}: {Subject}", toEmail, subject);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {ToEmail}: {Subject}. Error: {Error}", toEmail, subject, ex.Message);
                return false;
            }
        }
    }
}
