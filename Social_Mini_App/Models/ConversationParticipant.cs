using MiniSocialNetwork.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Social_Mini_App.Models
{
    public class ConversationParticipant
    {
        [Key]
        public Guid ParticipantId { get; set; }

        [Required]
        public Guid ConversationId { get; set; }
        
        [ForeignKey("ConversationId")]
        public virtual Conversation Conversation { get; set; } = null!;

        [Required]
        public Guid UserId { get; set; }
        
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        public DateTime JoinedAt { get; set; } = DateTime.Now;

        // Is this participant an admin of the group?
        public bool IsAdmin { get; set; } = false;
    }
}
