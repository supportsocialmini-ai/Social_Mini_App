using MiniSocialNetwork.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Social_Mini_App.Models
{
    public class GroupMember
    {
        [Required]
        public Guid GroupId { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        [StringLength(20)]
        public string Role { get; set; } = "Member"; // Owner, Admin, Member

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Active"; // Active, Pending

        [ForeignKey("GroupId")]
        public virtual Group? Group { get; set; }

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
    }
}
