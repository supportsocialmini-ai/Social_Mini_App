using Social_Mini_App.Models;

namespace Social_Mini_App.Interfaces
{
    public interface INotificationService
    {
        Task CreateNotifAsync(Guid senderId, Guid receiverId, Guid? postId, string type);
        Task<List<Notification>> GetNotificationsAsync(Guid userId);
        Task<bool> MarkAsReadAsync(Guid notificationId);
        Task<bool> MarkAllAsReadAsync(Guid userId);
    }
}