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
            // Bốc đúng thằng user theo ID từ bảng Users (có cả Email, FullName...)
            return await _context.Users.FirstOrDefaultAsync(u => u.UserId == id);
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