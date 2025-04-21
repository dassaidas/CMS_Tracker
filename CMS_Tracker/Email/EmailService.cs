using MimeKit;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CMS_Tracker.Models;
using Microsoft.Extensions.Logging;

namespace CMS_Tracker.Email
{
    public class EmailService : IEmailService
    {
        private readonly CMS_DBContext _context;
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;

        public EmailService(CMS_DBContext context, IConfiguration config, ILogger<EmailService> logger)
        {
            _context = context;
            _config = config;
            _logger = logger;
        }

        public async Task SendEmailAsync(string to, string templateName, Dictionary<string, string> placeholders)
        {
            try
            {
                var template = await _context.EmailTemplates
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.Name == templateName && t.IsActive == true);

                if (template == null)
                {
                    _logger.LogWarning("Email template '{TemplateName}' not found or inactive.", templateName);
                    throw new Exception($"Email template '{templateName}' not found or inactive.");
                }

                string subject = ReplacePlaceholders(template.Subject, placeholders);
                string body = ReplacePlaceholders(template.Body, placeholders);

                var message = new MimeMessage();
                message.From.Add(MailboxAddress.Parse(_config["Smtp:From"]));
                message.To.Add(MailboxAddress.Parse(to));
                message.Subject = subject;
                message.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = body };

                using var smtp = new SmtpClient();

                await smtp.ConnectAsync(
                    _config["Smtp:Host"],
                    int.Parse(_config["Smtp:Port"]),
                    MailKit.Security.SecureSocketOptions.StartTls
                );

                await smtp.AuthenticateAsync(
                    _config["Smtp:Username"],
                    _config["Smtp:Password"]
                );

                await smtp.SendAsync(message);
                _logger.LogInformation("Email sent to {To} using template '{Template}'", to, templateName);

                await smtp.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email to {To} using template '{Template}'", to, templateName);
                throw new Exception("Error sending email", ex);
            }
        }

        private string ReplacePlaceholders(string template, Dictionary<string, string> placeholders)
        {
            return placeholders.Aggregate(template, (current, pair) =>
                current.Replace($"{{{pair.Key}}}", pair.Value));
        }
    }
}