using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using MiniSocialNetwork.Data;
using Social_Mini_App.Hubs;
using Social_Mini_App.Interfaces;
using Social_Mini_App.Models;

namespace Social_Mini_App.Services
{
    public class NotificationService : INotificationService
    {
        private readonly DataContext _context;
        private readonly IHubContext<ChatHub> _hubContext;

        public NotificationService(DataContext context, IHubContext<ChatHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public async Task CreateNotifAsync(Guid senderId, Guid receiverId, Guid? postId, string type)
        {
            if (senderId == receiverId) return;
 
            var sender = await _context.Users.FindAsync(senderId);
            string action = "";
            
            if (type == "Like") action = "đã thích bài viết của bạn.";
            else if (type == "Comment") action = "đã bình luận vào bài viết của bạn.";
            else if (type == "FriendRequest") action = "đã gửi lời mời kết bạn cho bạn.";
            else if (type == "FriendAccept") action = "đã chấp nhận lời mời kết bạn.";
 
            var notif = new Notification
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                PostId = postId,
                NotificationType = type,
                Message = $"{sender?.FullName ?? sender?.Username} {action}",
                CreatedAt = DateTime.Now,
                IsRead = false
            };
 
            _context.Notifications.Add(notif);
            await _context.SaveChangesAsync();

            // Gửi thông báo realtime qua SignalR
            await _hubContext.Clients.User(receiverId.ToString()).SendAsync("ReceiveNotification", notif.Message);
        }
 
        // Thêm hàm lấy danh sách thông báo để tí nữa FE gọi nhé
        public async Task<List<Notification>> GetNotificationsAsync(Guid userId)
        {
            return await _context.Notifications
                .Where(n => n.ReceiverId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(20) // Lấy 20 cái mới nhất thôi
                .ToListAsync();
        }
 
        public async Task<bool> MarkAsReadAsync(Guid notificationId)
        {
            var notif = await _context.Notifications.FindAsync(notificationId);
            if (notif == null) return false;
 
            notif.IsRead = true;
            return await _context.SaveChangesAsync() > 0;
        }
 
        public async Task<bool> MarkAllAsReadAsync(Guid userId)
        {
            var unreadNotifs = await _context.Notifications
                .Where(n => n.ReceiverId == userId && !n.IsRead)
                .ToListAsync();

            if (!unreadNotifs.Any()) return true;

            foreach (var notif in unreadNotifs)
            {
                notif.IsRead = true;
            }

            return await _context.SaveChangesAsync() > 0;
        }
    }
}