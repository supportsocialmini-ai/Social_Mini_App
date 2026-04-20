using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using MiniSocialNetwork.Data;
using MiniSocialNetwork.Interfaces;
using MiniSocialNetwork.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.IO;
using Social_Mini_App.Messages;

public class AuthService : IAuthService
{
    private readonly DataContext _context = null!;
    private readonly IConfiguration _configuration = null!;
    private readonly IEmailService _mailService = null!;

    public AuthService(DataContext context, IConfiguration configuration, IEmailService mailService)
    {
        _context = context;
        _configuration = configuration;
        _mailService = mailService;
    }

    public async Task<string> RegisterAsync(User user, string password)
    {
        if (await _context.Users.AnyAsync(u => u.Username == user.Username))
            return AuthMsg.Register.UserExists;

        if (await _context.Users.AnyAsync(u => u.Email == user.Email))
            return AuthMsg.Register.EmailExists;

        // Lấy password từ tham số truyền vào để hash
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);

        user.CreatedAt = DateTime.Now;
        user.IsActive = false; // Chuyển thành false để chờ xác nhận mail
        user.IsVerified = false;
        user.VerificationToken = Random.Shared.Next(100000, 1000000).ToString();

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Gán Role mặc định là "User"
        var userRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "User");
        if (userRole == null)
        {
            userRole = new Role { RoleId = Guid.NewGuid(), Name = "User" };
            _context.Roles.Add(userRole);
            await _context.SaveChangesAsync();
        }

        _context.UserRoles.Add(new UserRole { UserId = user.UserId, RoleId = userRole.RoleId });
        await _context.SaveChangesAsync();

        // Gửi mail xác nhận ở chế độ chạy ngầm (Background Task) 
        // để không làm chậm quá trình đăng ký của người dùng
        _ = Task.Run(async () =>
        {
            try
            {
                // Gửi mail xác nhận
                var frontendUrl = _configuration["AppSettings:FrontendUrl"] ?? "http://localhost:3000";
                var verificationUrl = $"{frontendUrl.TrimEnd('/')}/verify-email?token={user.VerificationToken}";
                
                var placeholders = new Dictionary<string, string>
                {
                    { "FullName", user.FullName },
                    { "VerificationUrl", verificationUrl },
                    { "VerificationToken", user.VerificationToken ?? "" }
                };

                await _mailService.SendTemplateEmailAsync(user.Email, "Xác nhận tài khoản SocialMini", "VerifyEmail", placeholders);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Background Email Error: {ex.Message}");
            }
        });

        return AuthMsg.Register.Success;
    }

    public async Task<string?> LoginAsync(string username, string password)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            return null;

        if (!user.IsActive)
            throw new Exception(AuthMsg.Login.UserNotVerified);

        var roles = await _context.UserRoles
            .Where(ur => ur.UserId == user.UserId)
            .Select(ur => ur.Role.Name)
            .ToListAsync();

        return CreateToken(user, roles);
    }

    public async Task<bool> VerifyEmailAsync(string token)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.VerificationToken == token);
        if (user == null) return false;

        user.IsVerified = true;
        user.IsActive = true;
        user.VerificationToken = null; // Xóa token sau khi dùng

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ChangePasswordAsync(Guid userId, string oldPassword, string newPassword)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
        if (user == null || !BCrypt.Net.BCrypt.Verify(oldPassword, user.PasswordHash))
            return false;
 
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        await _context.SaveChangesAsync();
        return true;
    }
 
    public async Task<bool> VerifyPasswordAsync(Guid userId, string password)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
        if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            return false;
 
        return true;
    }

    public async Task<string> ForgotPasswordAsync(string email)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null) return AuthMsg.Password.UserNotFound;

        user.PasswordResetToken = Random.Shared.Next(100000, 1000000).ToString();
        user.ResetTokenExpires = DateTime.Now.AddHours(1);

        await _context.SaveChangesAsync();

        _ = Task.Run(async () =>
        {
            try
            {
                var placeholders = new Dictionary<string, string>
                {
                    { "FullName", user.FullName },
                    { "Token", user.PasswordResetToken }
                };
                await _mailService.SendTemplateEmailAsync(user.Email, "Đặt lại mật khẩu SocialMini", "ResetPassword", placeholders);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Forgot Password Email Error: {ex.Message}");
            }
        });

        return AuthMsg.Password.ForgotEmailSent;
    }

    public async Task<bool> ResetPasswordAsync(string token, string newPassword)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.PasswordResetToken == token && u.ResetTokenExpires > DateTime.Now);
        if (user == null) return false;

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        user.PasswordResetToken = null;
        user.ResetTokenExpires = null;

        await _context.SaveChangesAsync();
        return true;
    }

    private string CreateToken(User user, List<string> roles)
    {
        var claims = new List<Claim> {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Name, user.Username)
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("AppSettings:Token").Value!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.Now.AddDays(1),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}