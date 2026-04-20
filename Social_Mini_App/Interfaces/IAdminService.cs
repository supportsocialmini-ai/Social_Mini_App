using MiniSocialNetwork.Models;

namespace Social_Mini_App.Interfaces
{
    public interface IAdminService
    {
        Task<object> GetStatsAsync();
        Task<bool> GetMaintenanceStatusAsync();
        Task<bool> ToggleMaintenanceAsync();
        Task<IEnumerable<object>> GetAllUsersAsync();
        Task<bool> ToggleUserStatusAsync(Guid userId);
        Task<bool> DeletePostByAdminAsync(Guid postId);
    }
}
