using MiniSocialNetwork.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Social_Mini_App.Models
{
    public class Message
    {
        [Key]
        public Guid MessageId { get; set; }

        [Required]
        public Guid SenderId { get; set; }

        [Required]
        public Guid ConversationId { get; set; }

        [Required]
        public string MessageContent { get; set; } = string.Empty;

        public bool IsRead { get; set; } = false;
        public string? ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Connections
        [ForeignKey("SenderId")]
        public virtual User? Sender { get; set; }

        [ForeignKey("ConversationId")]
        public virtual Conversation? Conversation { get; set; }
    }
}
