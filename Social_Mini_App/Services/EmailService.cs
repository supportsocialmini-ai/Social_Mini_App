using SendGrid;
using SendGrid.Helpers.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MiniSocialNetwork.Interfaces;

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
            var apiKey = _configuration["SendGrid:ApiKey"];
            var fromEmail = _configuration["MailSettings:Email"] ?? "support.socialmini@gmail.com";
            var fromName = _configuration["MailSettings:DisplayName"] ?? "Social Mini Network";

            if (string.IsNullOrEmpty(apiKey))
            {
                _logger.LogError("SendGrid ApiKey is missing in configuration.");
                throw new Exception("Email service is not configured correctly (ApiKey missing).");
            }

            var client = new SendGridClient(apiKey);
            var from = new EmailAddress(fromEmail, fromName);
            var toEmail = new EmailAddress(to);
            var msg = MailHelper.CreateSingleEmail(from, toEmail, subject, "", body);

            try
            {
                _logger.LogInformation("Sending email via SendGrid API to {To}", to);
                var response = await client.SendEmailAsync(msg);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Email sent successfully via SendGrid to {To}", to);
                }
                else
                {
                    var errorBody = await response.Body.ReadAsStringAsync();
                    _logger.LogError("Failed to send email via SendGrid. Status: {Status}, Error: {Error}", response.StatusCode, errorBody);
                    throw new Exception($"SendGrid Error: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while sending email via SendGrid to {To}", to);
                throw;
            }
        }
    }
}
