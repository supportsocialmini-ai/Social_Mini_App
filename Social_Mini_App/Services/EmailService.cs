using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MiniSocialNetwork.Interfaces;
using System.Text;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;

namespace MiniSocialNetwork.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration = null!;
        private readonly ILogger<EmailService> _logger = null!;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var clientId = _configuration["GmailApi:ClientId"];
            var clientSecret = _configuration["GmailApi:ClientSecret"];
            var refreshToken = _configuration["GmailApi:RefreshToken"];
            var fromEmail = _configuration["MailSettings:Email"] ?? "support.socialmini@gmail.com";

            // LOG DEBUG: Kiểm tra giá trị đang nhận được (Che bớt ở giữa)
            if (!string.IsNullOrEmpty(clientId))
                _logger.LogInformation("DEBUG - ClientId: {Start}...{End}", clientId[..5], clientId[^10..]);
            
            if (!string.IsNullOrEmpty(refreshToken))
                _logger.LogInformation("DEBUG - RefreshToken: {Start}...{End}", refreshToken[..5], refreshToken[^5..]);

            // Chế độ DevMode: Gửi qua SMTP truyền thống (Dùng App Password)
            var devMode = _configuration.GetValue<bool>("MailSettings:DevMode");
            if (devMode)
            {
                _logger.LogInformation("[DEV MODE] Đang gửi email qua SMTP tới {To}...", to);
                await SendEmailViaSmtpAsync(to, subject, body);
                return;
            }

            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret) || string.IsNullOrEmpty(refreshToken))
            {
                _logger.LogError("Gmail API credentials are missing in configuration.");
                throw new Exception("Email service is not configured correctly (Gmail API credentials missing).");
            }

            try
            {
                var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
                {
                    ClientSecrets = new ClientSecrets
                    {
                        ClientId = clientId,
                        ClientSecret = clientSecret
                    }
                });

                var tokenResponse = new TokenResponse
                {
                    RefreshToken = refreshToken
                };

                var credential = new UserCredential(flow, "user", tokenResponse);

                var service = new GmailService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "SocialMini"
                });

                string encodedSubject = "=?utf-8?B?" + Convert.ToBase64String(Encoding.UTF8.GetBytes(subject)) + "?=";
                
                // Xây dựng nội dung email theo chuẩn MIME
                var mailContent = $"To: {to}\r\n" +
                                 $"Subject: {encodedSubject}\r\n" +
                                 $"Content-Type: text/html; charset=utf-8\r\n\r\n" +
                                 $"{body}";

                var message = new Message
                {
                    Raw = Base64UrlEncode(mailContent)
                };

                _logger.LogInformation("Sending email via Gmail API to {To}", to);
                await service.Users.Messages.Send(message, "me").ExecuteAsync();
                _logger.LogInformation("Email sent successfully via Gmail API to {To}", to);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while sending email via Gmail API to {To}", to);
                throw;
            }
        }

        public async Task SendTemplateEmailAsync(string to, string subject, string templateName, Dictionary<string, string> placeholders)
        {
            string body;
            try
            {
                var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "EmailTemplates", $"{templateName}.html");
                if (File.Exists(templatePath))
                {
                    body = await File.ReadAllTextAsync(templatePath);
                    foreach (var placeholder in placeholders)
                    {
                        body = body.Replace($"{{{placeholder.Key}}}", placeholder.Value);
                    }
                }
                else
                {
                    _logger.LogWarning("Email template {TemplateName} not found at {TemplatePath}", templateName, templatePath);
                    body = string.Join(", ", placeholders.Select(p => $"{p.Key}: {p.Value}"));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading or parsing email template {TemplateName}", templateName);
                body = string.Join(", ", placeholders.Select(p => $"{p.Key}: {p.Value}"));
            }

            await SendEmailAsync(to, subject, body);
        }

        private async Task SendEmailViaSmtpAsync(string to, string subject, string body)
        {
            try
            {
                var email = new MimeMessage();
                email.From.Add(new MailboxAddress(_configuration["MailSettings:DisplayName"] ?? "Social Mini", _configuration["MailSettings:Email"]));
                email.To.Add(MailboxAddress.Parse(to));
                email.Subject = subject;
                email.Body = new TextPart(TextFormat.Html) { Text = body };

                using var smtp = new SmtpClient();
                // Bỏ qua kiểm tra chứng chỉ SSL nếu cần thiết ở local (tùy chọn)
                // smtp.ServerCertificateValidationCallback = (s, c, h, e) => true;

                await smtp.ConnectAsync(
                    _configuration["MailSettings:Host"], 
                    int.Parse(_configuration["MailSettings:Port"] ?? "587"), 
                    SecureSocketOptions.StartTls
                );

                await smtp.AuthenticateAsync(
                    _configuration["MailSettings:Email"], 
                    _configuration["MailSettings:Password"]
                );

                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);
                
                _logger.LogInformation("SMTP: Email đã được gửi thành công tới {To}", to);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SMTP: Lỗi khi gửi email tới {To}", to);
                throw;
            }
        }

        private string Base64UrlEncode(string input)
        {
            var inputBytes = Encoding.UTF8.GetBytes(input);
            return Convert.ToBase64String(inputBytes)
                .Replace('+', '-')
                .Replace('/', '_')
                .Replace("=", "");
        }
    }
}
