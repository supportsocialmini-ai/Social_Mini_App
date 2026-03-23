using MiniSocialNetwork.Models;

namespace Social_Mini_App.Models
{
    public class Comment
    {
        public Guid CommentId { get; set; }
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