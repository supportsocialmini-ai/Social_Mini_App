using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using MiniSocialNetwork.Data;
using Social_Mini_App.Hubs;
using Social_Mini_App.Interfaces;
using Social_Mini_App.Models;
using Social_Mini_App.Messages;

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
 
            string actionKey = type switch
            {
                "Like" => NotificationMsg.Action.Like,
                "Comment" => NotificationMsg.Action.Comment,
                "FriendRequest" => NotificationMsg.Action.FriendRequest,
                "FriendAccept" => NotificationMsg.Action.FriendAccept,
                _ => type
            };
 
            var notif = new Notification
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                PostId = postId,
                NotificationType = type,
                Message = actionKey, // Store the KEY instead of the full sentence
                CreatedAt = DateTime.Now,
                IsRead = false
            };
 
            _context.Notifications.Add(notif);
            await _context.SaveChangesAsync();

            // Fetch sender again to get FullName for real-time notification
            var sender = await _context.Users.FindAsync(senderId);

            // Gửi "Mã thông báo|Tên người gửi" qua SignalR để FE dễ bóc tách
            string signalRMsg = $"{actionKey}|{sender?.FullName ?? sender?.Username}";
            await _hubContext.Clients.User(receiverId.ToString()).SendAsync("ReceiveNotification", signalRMsg);
        }
 
        public async Task<List<Notification>> GetNotificationsAsync(Guid userId)
        {
            return await _context.Notifications
                .Include(n => n.Sender)
                .Where(n => n.ReceiverId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(20)
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