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
    private readonly DataContext _context;
    private readonly IConfiguration _configuration;
    private readonly IEmailService _mailService;

    public AuthService(DataContext context, IConfiguration configuration, IEmailService mailService)
    {
        _context = context;
        _configuration = configuration;
        _mailService = mailService;
    }

    public async Task<string> RegisterAsync(User user, string password)
    {
        if (await _context.Users.AnyAsync(u => u.Username == user.Username))
            return "T├¬n ─æ─âng nhß║¡p ─æ├ú tß╗ôn tß║íi!";

        if (await _context.Users.AnyAsync(u => u.Email == user.Email))
            return "Email ─æ├ú ─æ╞░ß╗úc sß╗¡ dß╗Ñng!";

        // Lß║Ñy password tß╗½ tham sß╗æ truyß╗ün v├áo ─æß╗â hash
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);

        user.CreatedAt = DateTime.Now;
        user.IsActive = false; // Chuyß╗ân th├ánh false ─æß╗â chß╗¥ x├íc nhß║¡n mail
        user.IsVerified = false;
        user.VerificationToken = Random.Shared.Next(100000, 1000000).ToString();

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Gß╗¡i mail x├íc nhß║¡n
        var frontendUrl = _configuration["AppSettings:FrontendUrl"] ?? "http://localhost:3000";
        var verificationUrl = $"{frontendUrl.TrimEnd('/')}/verify-email?token={user.VerificationToken}";
        
        // ─Éß╗ìc template tß╗½ file
        string mailBody;
        try 
        {
            var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "EmailTemplates", "VerifyEmail.html");
            mailBody = File.ReadAllText(templatePath);
            mailBody = mailBody.Replace("{FullName}", user.FullName)
                               .Replace("{VerificationUrl}", verificationUrl)
                               .Replace("{VerificationToken}", user.VerificationToken);
        }
        catch (Exception ex)
        {
            // Fallback nß║┐u kh├┤ng ─æß╗ìc ─æ╞░ß╗úc file
            mailBody = $"Ch├áo {user.FullName}, m├ú x├íc nhß║¡n cß╗ºa bß║ín l├á: {user.VerificationToken}. Hoß║╖c nhß║Ñn v├áo link: {verificationUrl}";
        }

        // Gß╗¡i mail x├íc nhß║¡n ß╗ƒ chß║┐ ─æß╗Ö chß║íy ngß║ºm (Background Task) 
        // ─æß╗â kh├┤ng l├ám chß║¡m qu├í tr├¼nh ─æ─âng k├╜ cß╗ºa ng╞░ß╗¥i d├╣ng
        _ = Task.Run(async () =>
        {
            try
            {
                await _mailService.SendEmailAsync(user.Email, "X├íc nhß║¡n t├ái khoß║ún SocialMini", mailBody);
            }
            catch (Exception ex)
            {
                // Lß╗ùi gß╗¡i mail sß║╜ ─æ╞░ß╗úc Log trong EmailService rß╗ôi, 
                // ß╗ƒ ─æ├óy ta chß╗ë cß║ºn bß║»t exception ─æß╗â Task ngß║ºm kh├┤ng l├ám crash app
                Console.WriteLine($"Background Email Error: {ex.Message}");
            }
        });

        return "─É─âng k├╜ th├ánh c├┤ng! Vui l├▓ng kiß╗âm tra email ─æß╗â x├íc nhß║¡n t├ái khoß║ún.";
    }

    public async Task<string?> LoginAsync(string username, string password)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            return null;

        if (!user.IsActive)
            throw new Exception("T├ái khoß║ún ch╞░a ─æ╞░ß╗úc k├¡ch hoß║ít. Vui l├▓ng kiß╗âm tra email ─æß╗â x├íc nhß║¡n hoß║╖c nhß║¡p m├ú 6 chß╗» sß╗æ ─æß╗â x├íc nhß║¡n sß╗¡ dß╗Ñng");

        return CreateToken(user);
    }

    public async Task<bool> VerifyEmailAsync(string token)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.VerificationToken == token);
        if (user == null) return false;

        user.IsVerified = true;
        user.IsActive = true;
        user.VerificationToken = null; // X├│a token sau khi d├╣ng

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
