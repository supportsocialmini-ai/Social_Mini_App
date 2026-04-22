using Microsoft.EntityFrameworkCore;
using MiniSocialNetwork.Data;
using MiniSocialNetwork.Models;
using Social_Mini_App.Interfaces;

namespace Social_Mini_App.Services
{
    public class UserService : IUserService
    {
        private readonly DataContext _context;

        public UserService(DataContext context)
        {
            _context = context;
        }

        public async Task<User?> GetUserByIdAsync(Guid id)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .Include(u => u.Subscriptions)
                    .ThenInclude(s => s.Package)
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user != null)
            {
                // Tự động gom tất cả features từ các gói đang active
                user.ActiveFeatures = user.Subscriptions
                    .Where(s => s.IsActive && s.Package != null && !string.IsNullOrEmpty(s.Package.Features))
                    .SelectMany(s => s.Package.Features.Split(',', StringSplitOptions.RemoveEmptyEntries))
                    .Select(f => f.Trim())
                    .Distinct()
                    .ToList();
            }

            return user;
        }
        public async Task<bool> UpdateUserAsync(User user)
        {
            _context.Users.Update(user);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _context.Users.ToListAsync();
        }
    }
}