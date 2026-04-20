using MiniSocialNetwork.Models;
using System.ComponentModel.DataAnnotations;

namespace Social_Mini_App.Models
{
    public class Reply
    {
        public Guid ReplyId { get; set; }

        [Required(ErrorMessage = "Nội dung phản hồi không được để trống!")]
        [MaxLength(200, ErrorMessage = "Phản hồi quá dài, tối đa 200 ký tự nhé!")]
        public string ReplyContent { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Quan hệ với User
        public Guid UserId { get; set; }
        public virtual User? User { get; set; }

        // Quan hệ với Comment (Gốc)
        public Guid CommentId { get; set; }
        public virtual Comment? Comment { get; set; }
    }
}
