namespace MiniSocialNetwork.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
        Task SendTemplateEmailAsync(string to, string subject, string templateName, Dictionary<string, string> placeholders);
    }
}
