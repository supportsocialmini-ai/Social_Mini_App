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

        // 1. Lấy Newsfeed (Có LikeCount và IsLiked) - Đã lọc theo Privacy
        public async Task<List<PostResponse>> GetNewsfeedAsync(Guid currentUserId)
        {
            var friendsIds = await GetFriendsIdsAsync(currentUserId);

            var query = _context.Posts
                .Include(p => p.User)
                .Include(p => p.OriginalPost).ThenInclude(op => op!.User)
                .Include(p => p.Likes)
                .Include(p => p.Comments)
                .Where(p => p.UserId == currentUserId 
                         || p.Privacy == "Public" 
                         || (p.Privacy == "Friends" && friendsIds.Contains(p.UserId)));

            return await query
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
                    Privacy = p.Privacy,
                    LikeCount = p.Likes.Count(),
                    IsLiked = p.Likes.Any(l => l.UserId == currentUserId),
                    CommentCount = p.Comments.Count(),
                    OriginalPostId = p.OriginalPostId,
                    OriginalPost = p.OriginalPost == null ? null : new PostResponse
                    {
                        PostId = p.OriginalPost.PostId,
                        PostContent = p.OriginalPost.PostContent,
                        CreatedAt = p.OriginalPost.CreatedAt,
                        UserId = p.OriginalPost.UserId,
                        FullName = p.OriginalPost.User!.FullName ?? p.OriginalPost.User.Username,
                        AvatarUrl = p.OriginalPost.User.AvatarUrl,
                        ImageUrl = p.OriginalPost.ImageUrl,
                        Privacy = p.OriginalPost.Privacy
                    }
                })
                .ToListAsync();
        }

        // 2. Lấy bài viết của CHÍNH TÔI (Thấy hết)
        public async Task<List<PostResponse>> GetMyPostsAsync(Guid userId, Guid currentUserId)
        {
            return await _context.Posts
                .Include(p => p.User)
                .Include(p => p.OriginalPost).ThenInclude(op => op!.User)
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
                    Privacy = p.Privacy,
                    LikeCount = p.Likes.Count(),
                    IsLiked = p.Likes.Any(l => l.UserId == currentUserId),
                    CommentCount = p.Comments.Count(),
                    OriginalPostId = p.OriginalPostId,
                    OriginalPost = p.OriginalPost == null ? null : new PostResponse
                    {
                        PostId = p.OriginalPost.PostId,
                        PostContent = p.OriginalPost.PostContent,
                        CreatedAt = p.OriginalPost.CreatedAt,
                        UserId = p.OriginalPost.UserId,
                        FullName = p.OriginalPost.User!.FullName ?? p.OriginalPost.User.Username,
                        AvatarUrl = p.OriginalPost.User.AvatarUrl,
                        ImageUrl = p.OriginalPost.ImageUrl,
                        Privacy = p.OriginalPost.Privacy
                    }
                })
                .ToListAsync();
        }

        // 3. Lấy bài viết của người khác (Đã lọc theo Privacy)
        public async Task<List<PostResponse>> GetPostsByUserIdAsync(Guid userId, Guid currentUserId)
        {
            var isFriend = await _context.FriendshipMembers
                .Where(fm => fm.UserId == currentUserId)
                .Join(_context.Friendships.Where(f => f.Status == "Accepted"),
                      fm => fm.FriendshipId,
                      f => f.FriendshipId,
                      (fm, f) => fm.FriendshipId)
                .AnyAsync(fid => _context.FriendshipMembers.Any(fm2 => fm2.FriendshipId == fid && fm2.UserId == userId));

            return await _context.Posts
                .Include(p => p.User)
                .Include(p => p.OriginalPost).ThenInclude(op => op!.User)
                .Include(p => p.Likes)
                .Include(p => p.Comments)
                .Where(p => p.UserId == userId)
                .Where(p => p.UserId == currentUserId 
                         || p.Privacy == "Public" 
                         || (p.Privacy == "Friends" && isFriend))
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
                    Privacy = p.Privacy,
                    LikeCount = p.Likes.Count(),
                    IsLiked = p.Likes.Any(l => l.UserId == currentUserId),
                    CommentCount = p.Comments.Count(),
                    OriginalPostId = p.OriginalPostId,
                    OriginalPost = p.OriginalPost == null ? null : new PostResponse
                    {
                        PostId = p.OriginalPost.PostId,
                        PostContent = p.OriginalPost.PostContent,
                        CreatedAt = p.OriginalPost.CreatedAt,
                        UserId = p.OriginalPost.UserId,
                        FullName = p.OriginalPost.User!.FullName ?? p.OriginalPost.User.Username,
                        AvatarUrl = p.OriginalPost.User.AvatarUrl,
                        ImageUrl = p.OriginalPost.ImageUrl,
                        Privacy = p.OriginalPost.Privacy
                    }
                })
                .ToListAsync();
        }

        private async Task<List<Guid>> GetFriendsIdsAsync(Guid userId)
        {
            return await _context.FriendshipMembers
                .Where(fm => fm.UserId == userId)
                .Join(_context.Friendships.Where(f => f.Status == "Accepted"),
                      fm => fm.FriendshipId,
                      f => f.FriendshipId,
                      (fm, f) => f.FriendshipId)
                .SelectMany(fid => _context.FriendshipMembers
                    .Where(fm2 => fm2.FriendshipId == fid && fm2.UserId != userId)
                    .Select(fm2 => fm2.UserId))
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