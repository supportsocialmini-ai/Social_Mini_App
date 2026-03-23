using MiniSocialNetwork.Models;
using System.ComponentModel.DataAnnotations;

namespace Social_Mini_App.Models
{
    public class Comment
    {
        public Guid CommentId { get; set; }
        [Required(ErrorMessage = "Nội dung bình luận không được để trống!")]
        [MaxLength(1000, ErrorMessage = "Bình luận quá dài, tối đa 1000 ký tự thôi nhé!")]
        public string CommentContent { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Quan hệ với User
        public Guid UserId { get; set; }
        public virtual User? User { get; set; } = null!;

        // Quan hệ với Post
        public Guid PostId { get; set; }
        public virtual Post? Post { get; set; } = null!;

        // Quan hệ tự tham chiếu (Self-referencing) để làm Reply
        public Guid? ParentCommentId { get; set; }
        public virtual Comment? ParentComment { get; set; }
        public virtual ICollection<Comment> Replies { get; set; } = new List<Comment>();
    }
}