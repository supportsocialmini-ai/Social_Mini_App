using MiniSocialNetwork.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Social_Mini_App.Models
{
    public class Group
    {
        [Key]
        public Guid GroupId { get; set; }

        [Required(ErrorMessage = "Tên nhóm không được để trống!")]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public string? AvatarUrl { get; set; }
        public string? CoverUrl { get; set; }
        public string? Category { get; set; } // Ví dụ: "Công nghệ", "Thể thao", "Giải trí"

        [Required]
        [StringLength(20)]
        public string Privacy { get; set; } = "Public"; // Public, Private

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public Guid CreatedBy { get; set; }

        [ForeignKey("CreatedBy")]
        public virtual User? Creator { get; set; }

        public virtual ICollection<GroupMember> Members { get; set; } = new List<GroupMember>();
        public virtual ICollection<Post> Posts { get; set; } = new List<Post>();

        public virtual Conversation? Conversation { get; set; }
    }
}
