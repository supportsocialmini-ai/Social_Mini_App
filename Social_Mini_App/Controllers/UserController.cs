using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniSocialNetwork.Models;
using MiniSocialNetwork.Wrappers;
using Social_Mini_App.Dtos.Responses;
using Social_Mini_App.Dtos.Requests;
using Social_Mini_App.Interfaces;
using Social_Mini_App.Models;
using System.Security.Claims;
using Social_Mini_App.Messages;

using MiniSocialNetwork.Data;

namespace Social_Mini_App.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly DataContext _context;

        public UserController(IUserService userService, DataContext context)
        {
            _userService = userService;
            _context = context;
        }

        private Guid GetCurrentUserId()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (Guid.TryParse(userIdStr, out var userId)) return userId;
            return Guid.Empty;
        }

        [HttpGet]
        public async Task<IActionResult> GetProfile()
        {
            var userId = GetCurrentUserId();

            if (userId == Guid.Empty)
                return Unauthorized(ApiResponse<User>.Fail(UserMsg.Profile.Unauthorized));

            var user = await _userService.GetUserByIdAsync(userId);

            if (user == null) 
                return NotFound(ApiResponse<User>.Fail(UserMsg.Profile.NotFound));

            return Ok(ApiResponse<User>.Ok(user));
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetProfileById(Guid userId)
        {
            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null) 
                return NotFound(ApiResponse<User>.Fail(UserMsg.Profile.NotFound));

            return Ok(ApiResponse<User>.Ok(user));
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetUsers()
        {
            var currentUserId = GetCurrentUserId();

            // Lấy tất cả user (trừ user hiện tại nếu muốn, hoặc trả về hết để FE tự lọc)
            // Đồng thời JOIN với bảng Friendship/FriendshipMembers để xác định trạng thái quan hệ
            var userFriendships = await (
                from u in _context.Users
                where !u.IsDeleted
                let fmSelf = _context.FriendshipMembers.FirstOrDefault(fm => fm.UserId == currentUserId && _context.FriendshipMembers.Any(fm2 => fm2.UserId == u.UserId && fm2.FriendshipId == fm.FriendshipId))
                let f = fmSelf != null ? _context.Friendships.FirstOrDefault(fr => fr.FriendshipId == fmSelf.FriendshipId) : null
                select new
                {
                    u.UserId,
                    u.Username,
                    u.FullName,
                    u.AvatarUrl,
                    u.Category,
                    FriendshipStatus = f == null ? "None" : (f.Status == "Accepted" ? "Accepted" : (fmSelf.IsRequestSender ? "Sent" : "Received"))
                }
            ).ToListAsync();

            return Ok(ApiResponse<object>.Ok(userFriendships));
        }

        [HttpGet("interests")]
        public async Task<IActionResult> GetUniqueInterests()
        {
            var users = await _userService.GetAllUsersAsync();
            var allInterests = users
                .Select(u => u.Interests)
                .Where(interests => !string.IsNullOrEmpty(interests))
                .SelectMany(interests => interests.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                .Select(interest => interest.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(interest => interest)
                .ToList();

            return Ok(ApiResponse<List<string>>.Ok(allInterests));
        }

        [HttpPut]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty) return Unauthorized(ApiResponse<User>.Fail("Unauthorized"));

            var userInDb = await _userService.GetUserByIdAsync(userId);
            if (userInDb == null) return NotFound(ApiResponse<User>.Fail(UserMsg.Profile.NotFound));

            userInDb.FullName = request.FullName;
            if (!string.IsNullOrEmpty(request.Username)) userInDb.Username = request.Username;
            if (!string.IsNullOrEmpty(request.Email)) userInDb.Email = request.Email;
            if (!string.IsNullOrEmpty(request.AvatarUrl)) userInDb.AvatarUrl = request.AvatarUrl;
            if (!string.IsNullOrEmpty(request.Bio)) userInDb.Bio = request.Bio;
            userInDb.Gender = request.Gender;
            userInDb.DateOfBirth = request.DateOfBirth;
            userInDb.PhoneNumber = request.PhoneNumber;
            userInDb.Interests = request.Interests;
            userInDb.Category = request.Category;

            var result = await _userService.UpdateUserAsync(userInDb);
            if (result) 
                return Ok(ApiResponse<User>.Ok(userInDb));
                
            return BadRequest(ApiResponse<User>.Fail(UserMsg.Profile.UpdateFail));
        }

        [HttpPost("avatar")]
        public async Task<IActionResult> UploadAvatar(IFormFile file)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized(ApiResponse<object>.Fail("Unauthorized"));

            if (file == null || file.Length == 0)
                return BadRequest(ApiResponse<object>.Fail(UserMsg.Avatar.FileRequired));

            var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
            if (!allowedTypes.Contains(file.ContentType.ToLower()))
                return BadRequest(ApiResponse<object>.Fail(UserMsg.Avatar.InvalidType));

            var avatarDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "avatars");
            Directory.CreateDirectory(avatarDir);

            var ext = Path.GetExtension(file.FileName);
            var fileName = $"{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(avatarDir, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var avatarUrl = $"avatars/{fileName}";
            var userInDb = await _userService.GetUserByIdAsync(Guid.Parse(userIdStr));
            if (userInDb == null) return NotFound(ApiResponse<object>.Fail("User not found"));

            userInDb.AvatarUrl = avatarUrl;
            await _userService.UpdateUserAsync(userInDb);

            return Ok(ApiResponse<object>.Ok(new { avatarUrl, user = userInDb }));
        }

        [HttpDelete("me")]
        public async Task<IActionResult> DeleteProfile([FromBody] VerifyPasswordRequest request)
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty) return Unauthorized(ApiResponse<object>.Fail("Unauthorized"));

            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null) return NotFound(ApiResponse<object>.Fail(UserMsg.Profile.NotFound));

            // Xác nhận mật khẩu trước khi cho xóa
            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return BadRequest(ApiResponse<object>.Fail(AuthMsg.Password.VerifyFail));

            var result = await _userService.DeactivateUserAsync(userId);
            if (result)
                return Ok(ApiResponse<object>.Ok(new { message = "Tài khoản của bạn đã được vô hiệu hóa thành công." }));

            return BadRequest(ApiResponse<object>.Fail("Không thể vô hiệu hóa tài khoản lúc này."));
        }

        [HttpGet("Avatar/{fileName}")]
        [AllowAnonymous]
        public IActionResult GetAvatar(string fileName)
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "avatars", fileName);
            if (!System.IO.File.Exists(path))
            {
                path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", fileName);
                if (!System.IO.File.Exists(path)) return NotFound();
            }
            var contentType = fileName.EndsWith(".png") ? "image/png" :
                              fileName.EndsWith(".gif") ? "image/gif" :
                              fileName.EndsWith(".webp") ? "image/webp" : "image/jpeg";
            return PhysicalFile(path, contentType);
        }
    }
}