using System.ComponentModel.DataAnnotations;

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

        // Navigation properties
        public virtual ICollection<ConversationParticipant> Participants { get; set; } = new List<ConversationParticipant>();
        public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
    }
}
