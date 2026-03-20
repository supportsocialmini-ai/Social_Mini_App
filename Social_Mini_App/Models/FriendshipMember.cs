using MiniSocialNetwork.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Social_Mini_App.Models
{
    public class FriendshipMember
    {
        [Key]
        public Guid MemberId { get; set; }

        [Required]
        public Guid FriendshipId { get; set; }
        
        [ForeignKey("FriendshipId")]
        public virtual Friendship Friendship { get; set; } = null!;

        [Required]
        public Guid UserId { get; set; }
        
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        public bool IsRequestSender { get; set; } // To track who initiated the request
    }
}
