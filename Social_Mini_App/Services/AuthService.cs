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
            return "Tên đăng nhập đã tồn tại!";

        if (await _context.Users.AnyAsync(u => u.Email == user.Email))
            return "Email đã được sử dụng!";

        // Lấy password từ tham số truyền vào để hash
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);

        user.CreatedAt = DateTime.Now;
        user.IsActive = false; // Chuyển thành false để chờ xác nhận mail
        user.IsVerified = false;
        user.VerificationToken = Random.Shared.Next(100000, 1000000).ToString();

        _context.Users.Add(user);
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
                
                // Đọc template từ file
                string mailBody;
                try 
                {
                    var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "EmailTemplates", "VerifyEmail.html");
                    mailBody = File.ReadAllText(templatePath);
                    mailBody = mailBody.Replace("{FullName}", user.FullName)
                                       .Replace("{VerificationUrl}", verificationUrl)
                                       .Replace("{VerificationToken}", user.VerificationToken);
                }
                catch (Exception)
                {
                    // Fallback nếu không đọc được file
                    mailBody = $"Chào {user.FullName}, mã xác nhận của bạn là: {user.VerificationToken}. Hoặc nhấn vào link: {verificationUrl}";
                }

                await _mailService.SendEmailAsync(user.Email, "Xác nhận tài khoản SocialMini", mailBody);
            }
            catch (Exception ex)
            {
                // Lỗi gửi mail sẽ được Log trong EmailService rồi, 
                // ở đây ta chỉ cần bắt exception để Task ngầm không làm crash app
                Console.WriteLine($"Background Email Error: {ex.Message}");
            }
        });

        return "Đăng ký thành công! Vui lòng kiểm tra email để xác nhận tài khoản.";
    }

    public async Task<string?> LoginAsync(string username, string password)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            return null;

        if (!user.IsActive)
            throw new Exception("USER_NOT_VERIFIED");

        return CreateToken(user);
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

    private string CreateToken(User user)
    {
        var claims = new List<Claim> {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Name, user.Username)
        };

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