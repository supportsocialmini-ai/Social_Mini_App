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
