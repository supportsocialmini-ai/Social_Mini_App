using MiniSocialNetwork.Models;

namespace Social_Mini_App.Interfaces
{
    public interface IUserService
    {
        Task<User?> GetUserByIdAsync(Guid id);
        Task<bool> UpdateUserAsync(User user);
        Task<List<User>> GetAllUsersAsync();
    }
}