using Microsoft.EntityFrameworkCore;
using MiniSocialNetwork.Data;
using Social_Mini_App.Interfaces;
using Social_Mini_App.Models;

namespace Social_Mini_App.Services
{
    public class LikeService : ILikeService
    {
        private readonly DataContext _context;
        private readonly INotificationService _notifService;

        public LikeService(DataContext context, INotificationService notifService)
        {
            _context = context;
            _notifService = notifService;
        }

        public async Task<bool> ToggleLikeAsync(Guid postId, Guid userId)
        {
            // 1. Kiểm tra xem thằng này đã Like bài này chưa
            var existingLike = await _context.Likes
                .FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == userId);

            var post = await _context.Posts.FindAsync(postId);
            if (post == null) return false;

            if (existingLike != null)
            {
                // Nếu đã like rồi thì XÓA (Unliked)
                _context.Likes.Remove(existingLike);
                await _context.SaveChangesAsync();
                return false;
            }
            else
            {
                // Nếu chưa like thì THÊM (Liked)
                var newLike = new Like
                {
                    PostId = postId,
                    UserId = userId,
                    CreatedAt = DateTime.Now
                };
                _context.Likes.Add(newLike);

                // 2. TẠO THÔNG BÁO TỰ ĐỘNG
                // senderId là thằng đi like, receiverId là chủ bài post
                await _notifService.CreateNotifAsync(userId, post.UserId, postId, "Like");

                await _context.SaveChangesAsync();
                return true;
            }
        }
    }
}