using MiniSocialNetwork.Models;

namespace Social_Mini_App.Models
{
    public class Notification
    {
        public Guid NotificationId { get; set; }
        public Guid ReceiverId { get; set; } // Người nhận (Chủ bài viết)
        public Guid SenderId { get; set; }   // Người thực hiện hành động (Thằng đi like/cmt)
        public string Message { get; set; } = string.Empty; // "Nguyễn Văn A đã thích bài viết của bạn"
        public Guid? PostId { get; set; }     // Để bấm vào thông báo thì nhảy đến bài viết
        public string NotificationType { get; set; } = string.Empty; // "Like" hoặc "Comment"
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public virtual User? Sender { get; set; }
    }
}