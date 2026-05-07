using MiniSocialNetwork.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Social_Mini_App.Models
{
    public class Share
    {
        [Key]
        public Guid ShareId { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        [Required]
        public Guid PostId { get; set; }

        [ForeignKey("PostId")]
        public virtual Post? OriginalPost { get; set; }

        [MaxLength(500)]
        public string? Content { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Nếu share vào một cộng đồng cụ thể
        public Guid? GroupId { get; set; }
        
        [ForeignKey("GroupId")]
        public virtual Group? Group { get; set; }
    }
}
