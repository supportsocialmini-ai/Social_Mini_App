using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using MiniSocialNetwork.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MiniSocialNetwork.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var emailHost = _configuration["MailSettings:Host"] ?? "smtp.gmail.com";
            var emailPortStr = _configuration["MailSettings:Port"] ?? "587";
            var emailUser = _configuration["MailSettings:Email"];
            var emailPass = _configuration["MailSettings:Password"];

            if (string.IsNullOrEmpty(emailUser) || string.IsNullOrEmpty(emailPass))
            {
                _logger.LogError("Email configuration is missing (Email or Password is null/empty).");
                throw new Exception("Email service is not configured correctly.");
            }

            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(emailUser));
            email.To.Add(MailboxAddress.Parse(to));
            email.Subject = subject;

            var builder = new BodyBuilder { HtmlBody = body };
            email.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();
            try
            {
                int port = int.Parse(emailPortStr);
                // Nếu dùng port 465 thì thường là SslOnConnect, còn 587 là StartTls
                var socketOptions = port == 465 ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls;

                _logger.LogInformation("Connecting to SMTP server {Host}:{Port} using {Options}", emailHost, port, socketOptions);
                await smtp.ConnectAsync(emailHost, port, socketOptions);
                
                _logger.LogInformation("Authenticating as {User}", emailUser);
                await smtp.AuthenticateAsync(emailUser, emailPass);
                
                await smtp.SendAsync(email);
                _logger.LogInformation("Email sent successfully to {To}", to);
                
                await smtp.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {To} via {Host}:{Port}", to, emailHost, emailPortStr);
                throw; // Rethrow để AuthService bắt được và thông báo cho người dùng
            }
        }
    }
}
