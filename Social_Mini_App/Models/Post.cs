using MiniSocialNetwork.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Social_Mini_App.Models
{
    public class Post
    {
        [Key] // Khóa chính PostID
        public Guid PostId { get; set; }

        [Required]
        public Guid UserId { get; set; } // Người đăng bài (Khóa ngoại)

        [Required(ErrorMessage = "Nội dung bài viết không được để trống!")]
        [MaxLength(1000, ErrorMessage = "Bài viết quá dài, tối đa 1000 ký tự thôi nè!")]
        public string PostContent { get; set; } = string.Empty; // Nội dung bài viết

        [Required]
        [StringLength(20)]
        public string Privacy { get; set; } = "Public"; // Công khai / Bạn bè / Riêng tư

        public string? ImageUrl { get; set; } // URL ảnh bài viết (null nếu không có ảnh)

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Quan hệ: Một bài viết thuộc về một User
        // Dùng [JsonIgnore] để khi lấy bài viết nó không lôi cả cục User ra gây vòng lặp vô tận
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
        public ICollection<Like> Likes { get; set; } = new List<Like>();
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

        // Chức năng Share bài viết
        public Guid? OriginalPostId { get; set; }
        [ForeignKey("OriginalPostId")]
        public virtual Post? OriginalPost { get; set; }
    }
}
