using MiniSocialNetwork.Models;

namespace Social_Mini_App.Models
{
    public class Like
    {
        public Guid UserId { get; set; }

        public virtual User User { get; set; } = null!;

        public Guid PostId { get; set; }

        public virtual Post Post { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}