using Microsoft.EntityFrameworkCore;
using MiniSocialNetwork.Data;
using Social_Mini_App.Dtos.Responses;
using Social_Mini_App.Interfaces;
using Social_Mini_App.Models;

namespace Social_Mini_App.Services
{
    public class PostService : IPostService
    {
        private readonly DataContext _context;
        public PostService(DataContext context) => _context = context;

        // 1. Lấy Newsfeed (Có LikeCount và IsLiked)
        public async Task<List<PostResponse>> GetNewsfeedAsync(Guid currentUserId)
        {
            return await _context.Posts
                .Include(p => p.User)
                .Include(p => p.Likes)
                .Include(p => p.Comments) // Đừng quên Include bảng Comments nhé
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new PostResponse
                {
                    PostId = p.PostId,
                    PostContent = p.PostContent,
                    CreatedAt = p.CreatedAt,
                    UserId = p.UserId,
                    FullName = p.User!.FullName ?? p.User.Username,
                    AvatarUrl = p.User.AvatarUrl,
                    ImageUrl = p.ImageUrl,
                    LikeCount = p.Likes.Count(),
                    IsLiked = p.Likes.Any(l => l.UserId == currentUserId),

                    // ĐẾM SỐ BÌNH LUẬN Ở ĐÂY KKK
                    CommentCount = p.Comments.Count()
                })
                .ToListAsync();
        }

        // 2. Lấy MyPosts (Cũng phải trả về PostResponse để UI hiển thị được Like)
        public async Task<List<PostResponse>> GetMyPostsAsync(Guid userId, Guid currentUserId)
        {
            return await _context.Posts
                .Include(p => p.User)
                .Include(p => p.Likes)
                .Include(p => p.Comments)
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new PostResponse
                {
                    PostId = p.PostId,
                    PostContent = p.PostContent,
                    CreatedAt = p.CreatedAt,
                    UserId = p.UserId,
                    FullName = p.User!.FullName ?? p.User.Username,
                    AvatarUrl = p.User.AvatarUrl,
                    ImageUrl = p.ImageUrl,
                    LikeCount = p.Likes.Count(),
                    // SỬA CHỖ NÀY: Dùng currentUserId cho đồng bộ với Newsfeed
                    IsLiked = p.Likes.Any(l => l.UserId == currentUserId),
                    CommentCount = p.Comments.Count()
                })
                .ToListAsync();
        }

        // 3. Lấy bài viết của người khác theo ID
        public async Task<List<PostResponse>> GetPostsByUserIdAsync(Guid userId, Guid currentUserId)
        {
            return await _context.Posts
                .Include(p => p.User)
                .Include(p => p.Likes)
                .Include(p => p.Comments)
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new PostResponse
                {
                    PostId = p.PostId,
                    PostContent = p.PostContent,
                    CreatedAt = p.CreatedAt,
                    UserId = p.UserId,
                    FullName = p.User!.FullName ?? p.User.Username,
                    AvatarUrl = p.User.AvatarUrl,
                    ImageUrl = p.ImageUrl,
                    LikeCount = p.Likes.Count(),
                    IsLiked = p.Likes.Any(l => l.UserId == currentUserId),
                    CommentCount = p.Comments.Count()
                })
                .ToListAsync();
        }

        private async Task<List<PostResponse>> GetPostsInternal(IQueryable<Post> query, Guid currentUserId)
        {
            return await query
                .Include(p => p.User)
                .Include(p => p.Likes)
                .Include(p => p.Comments)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new PostResponse
                {
                    PostId = p.PostId,
                    PostContent = p.PostContent,
                    CreatedAt = p.CreatedAt,
                    UserId = p.UserId,
                    FullName = p.User!.FullName ?? p.User.Username,
                    AvatarUrl = p.User.AvatarUrl,
                    ImageUrl = p.ImageUrl,
                    LikeCount = p.Likes.Count(),
                    IsLiked = p.Likes.Any(l => l.UserId == currentUserId),
                    CommentCount = p.Comments.Count()
                })
                .ToListAsync();
        }

        public async Task<Post?> GetPostByIdAsync(Guid id)
            => await _context.Posts.FindAsync(id);

        public async Task<bool> CreatePostAsync(Post post)
        {
            _context.Posts.Add(post);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdatePostAsync(Post post)
        {
            _context.Posts.Update(post);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeletePostAsync(Guid id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null) return false;
            _context.Posts.Remove(post);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<List<UserSummaryDto>> GetPostLikesAsync(Guid postId)
        {
            return await _context.Likes
                .Where(l => l.PostId == postId)
                .Include(l => l.User)
                .Select(l => new UserSummaryDto
                {
                    UserId = l.User!.UserId,
                    Username = l.User.Username,
                    FullName = l.User.FullName,
                    AvatarUrl = l.User.AvatarUrl
                })
                .ToListAsync();
        }
    }
}