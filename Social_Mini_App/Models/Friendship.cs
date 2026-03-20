using System.ComponentModel.DataAnnotations;

namespace Social_Mini_App.Models
{
    public class Friendship
    {
        [Key]
        public Guid FriendshipId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? RequestedAt { get; set; }
        public DateTime? AcceptedAt { get; set; }

        [MaxLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, Accepted, Declined, Blocked

        // Navigation properties
        public virtual ICollection<FriendshipMember> Members { get; set; } = new List<FriendshipMember>();
    }
}
