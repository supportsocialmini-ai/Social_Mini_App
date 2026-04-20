using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniSocialNetwork.Data;
using MiniSocialNetwork.Models;
using MiniSocialNetwork.Wrappers;
using Social_Mini_App.Interfaces;

namespace Social_Mini_App.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IUserService _userService;

        public AdminController(DataContext context, IUserService _userService)
        {
            _context = context;
            this._userService = _userService;
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var totalUsers = await _context.Users.CountAsync();
            var totalPosts = await _context.Posts.CountAsync();
            var totalComments = await _context.Comments.CountAsync() + await _context.Replies.CountAsync();
            var activeUsers = await _context.Users.CountAsync(u => u.IsActive);

            return Ok(ApiResponse<object>.Ok(new
            {
                TotalUsers = totalUsers,
                TotalPosts = totalPosts,
                TotalComments = totalComments,
                ActiveUsers = activeUsers
            }));
        }

        [AllowAnonymous]
        [HttpGet("maintenance-status")]
        public async Task<ActionResult<object>> GetMaintenanceStatus()
        {
            var setting = await _context.SystemSettings.FirstOrDefaultAsync(s => s.Key == "MaintenanceMode");
            return Ok(new { isMaintenance = setting?.Value?.ToLower() == "true" });
        }

        [HttpPost("toggle-maintenance")]
        public async Task<ActionResult<object>> ToggleMaintenance()
        {
            var setting = await _context.SystemSettings.FirstOrDefaultAsync(s => s.Key == "MaintenanceMode");
            if (setting == null) return NotFound();

            bool currentStatus = setting.Value.ToLower() == "true";
            setting.Value = (!currentStatus).ToString().ToLower();
            setting.LastModified = DateTime.Now;

            await _context.SaveChangesAsync();
            return Ok(new { isMaintenance = setting.Value == "true" });
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _context.Users
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

            return Ok(ApiResponse<object>.Ok(users));
        }

        [HttpPost("users/{userId}/toggle-status")]
        public async Task<IActionResult> ToggleUserStatus(Guid userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound(ApiResponse<string>.Fail("Không tìm thấy người dùng"));

            user.IsActive = !user.IsActive;
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<bool>.Ok(user.IsActive));
        }

        [HttpDelete("posts/{postId}")]
        public async Task<IActionResult> DeletePostByAdmin(Guid postId)
        {
            var post = await _context.Posts.FindAsync(postId);
            if (post == null) return NotFound(ApiResponse<string>.Fail("Không tìm thấy bài viết"));

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<string>.Ok("Đã xóa bài viết bởi Admin"));
        }
    }
}
