using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniSocialNetwork.Interfaces;
using MiniSocialNetwork.Models;
using MiniSocialNetwork.Wrappers;
using Social_Mini_App.Dtos.Requests;
using Social_Mini_App.Messages;

namespace Social_Mini_App.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
    private readonly IAuthService _authService;
    public AuthController(IAuthService authService) => _authService = authService;

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                FullName = request.FullName,
                CreatedAt = DateTime.Now
            };

            var result = await _authService.RegisterAsync(user, request.Password);

            if (result == AuthMsg.Register.Success)
                return Ok(ApiResponse<string>.Ok(result));

            return BadRequest(ApiResponse<string>.Fail(result));
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                var token = await _authService.LoginAsync(request.Username, request.Password);
                if (token == null)
                    return BadRequest(ApiResponse<string>.Fail(AuthMsg.Login.Fail));

                return Ok(ApiResponse<string>.Ok(token));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.Fail(ex.Message));
            }
        }

        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromQuery] string token)
        {
            var result = await _authService.VerifyEmailAsync(token);
            if (result)
                return Ok(ApiResponse<string>.Ok(AuthMsg.Verify.Success));

            return BadRequest(ApiResponse<string>.Fail(AuthMsg.Verify.Fail));
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userIdStr == null || !Guid.TryParse(userIdStr, out var userId))
                return Unauthorized(ApiResponse<string>.Fail(UserMsg.Profile.Unauthorized));

            var result = await _authService.ChangePasswordAsync(userId, request.OldPassword, request.NewPassword);
            if (result)
                return Ok(ApiResponse<string>.Ok(AuthMsg.Password.ChangeSuccess));

            return BadRequest(ApiResponse<string>.Fail(AuthMsg.Password.ChangeFail));
        }

        [Authorize]
        [HttpPost("verify-password")]
        public async Task<IActionResult> VerifyPassword([FromBody] VerifyPasswordRequest request)
        {
            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userIdStr == null || !Guid.TryParse(userIdStr, out var userId))
                return Unauthorized(ApiResponse<bool>.Fail(UserMsg.Profile.Unauthorized));

            var result = await _authService.VerifyPasswordAsync(userId, request.Password);
            if (result)
                return Ok(ApiResponse<string>.Ok(AuthMsg.Password.VerifySuccess));

            return BadRequest(ApiResponse<string>.Fail(AuthMsg.Password.VerifyFail));
        }
    }
}