using MiniSocialNetwork.Models;
using System.ComponentModel.DataAnnotations;

namespace Social_Mini_App.Models
{
    public class Comment
    {
        public Guid CommentId { get; set; }
        [Required(ErrorMessage = "Nội dung bình luận không được để trống!")]
        [MaxLength(200, ErrorMessage = "Bình luận quá dài, tối đa 200 ký tự thôi nhé!")]
        public string CommentContent { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Quan hệ với User
        public Guid UserId { get; set; }
        public virtual User? User { get; set; } = null!;

        // Quan hệ với Post
        public Guid PostId { get; set; }
        public virtual Post? Post { get; set; } = null!;

        // Quan hệ với Reply (Tách ra để không tự tham chiếu)
        public virtual ICollection<Reply> Replies { get; set; } = new List<Reply>();
    }
}