using MiniSocialNetwork.Models;

namespace MiniSocialNetwork.Interfaces
{
    public interface IAuthService
    {
        Task<string> RegisterAsync(User user, string password); // Nhận object user và password riêng để hash
        Task<string?> LoginAsync(string username, string password);
        Task<bool> VerifyEmailAsync(string token);
        Task<bool> ChangePasswordAsync(Guid userId, string oldPassword, string newPassword);
        Task<bool> VerifyPasswordAsync(Guid userId, string password);
    }
}