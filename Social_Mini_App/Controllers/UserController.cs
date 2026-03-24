using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniSocialNetwork.Models;
using MiniSocialNetwork.Wrappers;
using Social_Mini_App.Dtos.Responses;
using Social_Mini_App.Dtos.Requests;
using Social_Mini_App.Interfaces;
using Social_Mini_App.Models;
using System.Security.Claims;
using Social_Mini_App.Messages;

namespace Social_Mini_App.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
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
            var users = await _userService.GetAllUsersAsync();
            var summaries = users.Select(u => new UserSummaryDto
            {
                UserId = u.UserId,
                Username = u.Username,
                FullName = u.FullName,
                AvatarUrl = u.AvatarUrl
            }).ToList();
            
            return Ok(ApiResponse<List<UserSummaryDto>>.Ok(summaries));
        }

        [HttpPut]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty) return Unauthorized(ApiResponse<User>.Fail("Unauthorized"));

            var userInDb = await _userService.GetUserByIdAsync(userId);
            if (userInDb == null) return NotFound(ApiResponse<User>.Fail(UserMsg.Profile.NotFound));

            userInDb.FullName = request.FullName;
            if (!string.IsNullOrEmpty(request.Email)) userInDb.Email = request.Email;
            if (!string.IsNullOrEmpty(request.AvatarUrl)) userInDb.AvatarUrl = request.AvatarUrl;
            if (!string.IsNullOrEmpty(request.Bio)) userInDb.Bio = request.Bio;

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