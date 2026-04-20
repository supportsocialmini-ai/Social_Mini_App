using MiniSocialNetwork.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Social_Mini_App.Models
{
    public class Conversation
    {
        [Key]
        public Guid ConversationId { get; set; }
        
        [MaxLength(200)]
        public string? Title { get; set; }
        
        public bool IsGroupChat { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Group chat fields
        public Guid? CreatorId { get; set; }

        [ForeignKey("CreatorId")]
        public virtual User? Creator { get; set; }

        [MaxLength(500)]
        public string? AvatarUrl { get; set; }


        // Navigation properties
        public virtual ICollection<ConversationParticipant> Participants { get; set; } = new List<ConversationParticipant>();
        public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
    }
}
