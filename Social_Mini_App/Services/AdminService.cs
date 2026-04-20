using Microsoft.EntityFrameworkCore;
using MiniSocialNetwork.Data;
using MiniSocialNetwork.Models;
using Social_Mini_App.Interfaces;

namespace Social_Mini_App.Services
{
    public class AdminService : IAdminService
    {
        private readonly DataContext _context;

        public AdminService(DataContext context)
        {
            _context = context;
        }

        public async Task<object> GetStatsAsync()
        {
            var totalUsers = await _context.Users.CountAsync();
            var totalPosts = await _context.Posts.CountAsync();
            var totalComments = await _context.Comments.CountAsync() + await _context.Replies.CountAsync();
            var activeUsers = await _context.Users.CountAsync(u => u.IsActive);

            return new
            {
                TotalUsers = totalUsers,
                TotalPosts = totalPosts,
                TotalComments = totalComments,
                ActiveUsers = activeUsers
            };
        }

        public async Task<bool> GetMaintenanceStatusAsync()
        {
            var setting = await _context.SystemSettings.FirstOrDefaultAsync(s => s.Key == "MaintenanceMode");
            return setting?.Value?.ToLower() == "true";
        }

        public async Task<bool> ToggleMaintenanceAsync()
        {
            var setting = await _context.SystemSettings.FirstOrDefaultAsync(s => s.Key == "MaintenanceMode");
            if (setting == null) return false;

            bool currentStatus = setting.Value.ToLower() == "true";
            setting.Value = (!currentStatus).ToString().ToLower();
            setting.LastModified = DateTime.Now;

            await _context.SaveChangesAsync();
            return setting.Value == "true";
        }

        public async Task<IEnumerable<object>> GetAllUsersAsync()
        {
            return await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .OrderByDescending(u => u.CreatedAt)
                .Select(u => new
                {
                    u.UserId,
                    u.Username,
                    u.FullName,
                    u.Email,
                    u.AvatarUrl,
                    u.IsActive,
                    u.IsVerified,
                    u.CreatedAt,
                    Roles = u.UserRoles.Select(ur => ur.Role.Name).ToList()
                })
                .ToListAsync();
        }

        public async Task<bool> ToggleUserStatusAsync(Guid userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            user.IsActive = !user.IsActive;
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeletePostByAdminAsync(Guid postId)
        {
            var post = await _context.Posts.FindAsync(postId);
            if (post == null) return false;

            _context.Posts.Remove(post);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
