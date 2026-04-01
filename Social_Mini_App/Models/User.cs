using Social_Mini_App.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization; // Thêm cái này

namespace MiniSocialNetwork.Models
{
    public class User
    {
        [Key]
        // [JsonIgnore] // Nếu muốn ẩn hoàn toàn khỏi Swagger, nhưng thường để mặc định cũng được
        public Guid UserId { get; set; }

        [Required]
        public string Username { get; set; } = string.Empty;

        // Ẩn cái PasswordHash đi vì Client không bao giờ gửi cái này lên
        [JsonIgnore]
        public string PasswordHash { get; set; } = string.Empty;

        // Dùng cái này để nhận mật khẩu từ Swagger/Postman
        [NotMapped]
        public string Password { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string? AvatarUrl { get; set; }
        [MaxLength(255)]
        public string? Bio { get; set; }

        public bool IsVerified { get; set; } = false;
        public bool IsActive { get; set; } = true;
        public string? VerificationToken { get; set; }
        public string? PasswordResetToken { get; set; }
        public DateTime? ResetTokenExpires { get; set; }

        // Ẩn luôn CreatedAt vì Server tự gán giờ hệ thống
        [JsonIgnore]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        public ICollection<Like> Likes { get; set; } = new List<Like>();
    }
}