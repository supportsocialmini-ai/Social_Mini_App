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

        public async Task<List<PostResponse>> GetNewsfeedAsync(Guid currentUserId, int page = 1, int pageSize = 10)
        {
            var friendsIds = await GetFriendsIdsAsync(currentUserId);
            var joinedGroupIds = await _context.GroupMembers
                .Where(gm => gm.UserId == currentUserId)
                .Select(gm => gm.GroupId)
                .ToListAsync();

            // 1. Phân trang hoàn toàn ở cấp Database sử dụng Union các ID và CreatedAt
            var postsQuery = _context.Posts
                .Where(p => (p.GroupId == null || joinedGroupIds.Contains(p.GroupId.Value)) &&
                         (p.UserId == currentUserId 
                           || p.Privacy == "Public" 
                           || (p.Privacy == "Friends" && friendsIds.Contains(p.UserId))))
                .Select(p => new { 
                    Id = p.PostId, 
                    CreatedAt = p.CreatedAt, 
                    IsShare = false,
                    IsSponsored = p.IsSponsored && (p.SponsorEndDate == null || p.SponsorEndDate > DateTime.UtcNow)
                });

            var sharesQuery = _context.Shares
                .Where(s => (s.GroupId == null || joinedGroupIds.Contains(s.GroupId.Value)) &&
                         (s.UserId == currentUserId 
                          || s.OriginalPost!.Privacy == "Public" 
                          || (s.OriginalPost.Privacy == "Friends" && friendsIds.Contains(s.UserId))))
                .Select(s => new { 
                    Id = s.ShareId, 
                    CreatedAt = s.CreatedAt, 
                    IsShare = true,
                    IsSponsored = false
                });

            var paginatedItems = await postsQuery.Union(sharesQuery)
                .OrderByDescending(x => x.IsSponsored)
                .ThenByDescending(x => x.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            if (!paginatedItems.Any())
            {
                return new List<PostResponse>();
            }

            var postIds = paginatedItems.Where(x => !x.IsShare).Select(x => x.Id).ToList();
            var shareIds = paginatedItems.Where(x => x.IsShare).Select(x => x.Id).ToList();

            // 2. Chỉ query dữ liệu chi tiết cho đúng các items được chọn
            List<PostResponse> posts = new List<PostResponse>();
            if (postIds.Any())
            {
                posts = await _context.Posts
                    .Where(p => postIds.Contains(p.PostId))
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
                        GroupId = p.GroupId,
                        GroupName = p.Group != null ? p.Group.Name : null,
                        IsShare = false,
                        IsSponsored = p.IsSponsored,
                        SponsorEndDate = p.SponsorEndDate
                    })
                    .ToListAsync();
            }

            List<PostResponse> shares = new List<PostResponse>();
            if (shareIds.Any())
            {
                shares = await _context.Shares
                    .Where(s => shareIds.Contains(s.ShareId))
                    .Select(s => new PostResponse
                    {
                        PostId = s.ShareId,
                        PostContent = string.Empty,
                        CreatedAt = s.CreatedAt,
                        UserId = s.UserId,
                        FullName = s.User!.FullName ?? s.User.Username,
                        AvatarUrl = s.User.AvatarUrl,
                        ImageUrl = null,
                        Privacy = s.OriginalPost!.Privacy,
                        LikeCount = s.OriginalPost.Likes.Count(),
                        IsLiked = s.OriginalPost.Likes.Any(l => l.UserId == currentUserId),
                        CommentCount = s.OriginalPost.Comments.Count(),
                        GroupId = s.GroupId,
                        GroupName = s.Group != null ? s.Group.Name : null,
                        IsShare = true,
                        ShareId = s.ShareId,
                        ShareContent = s.Content,
                        OriginalPostId = s.PostId,
                        OriginalPost = new PostResponse
                        {
                            PostId = s.OriginalPost.PostId,
                            PostContent = s.OriginalPost.PostContent,
                            CreatedAt = s.OriginalPost.CreatedAt,
                            UserId = s.OriginalPost.UserId,
                            FullName = s.OriginalPost.User!.FullName ?? s.OriginalPost.User.Username,
                            AvatarUrl = s.OriginalPost.User.AvatarUrl,
                            ImageUrl = s.OriginalPost.ImageUrl,
                            Privacy = s.OriginalPost.Privacy,
                            IsSponsored = s.OriginalPost.IsSponsored,
                            SponsorEndDate = s.OriginalPost.SponsorEndDate
                        }
                    })
                    .ToListAsync();
            }

            // 3. Kết hợp và sắp xếp: Ưu tiên bài viết được quảng cáo (IsSponsored = true và chưa hết hạn) lên trên, sau đó xếp theo thời gian CreatedAt giảm dần
            var combined = posts.Concat(shares)
                .OrderByDescending(p => p.IsSponsored && (p.SponsorEndDate == null || p.SponsorEndDate > DateTime.UtcNow))
                .ThenByDescending(p => p.CreatedAt)
                .ToList();

            return combined;
        }

        // 2. Lấy bài viết của CHÍNH TÔI
        public async Task<List<PostResponse>> GetMyPostsAsync(Guid userId, Guid currentUserId)
        {
            var posts = await _context.Posts
                .Where(p => p.UserId == userId)
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
                    GroupId = p.GroupId,
                    GroupName = p.Group != null ? p.Group.Name : null,
                    IsShare = false,
                    IsSponsored = p.IsSponsored,
                    SponsorEndDate = p.SponsorEndDate
                })
                .ToListAsync();

            var shares = await _context.Shares
                .Where(s => s.UserId == userId)
                .Select(s => new PostResponse
                {
                    PostId = s.ShareId,
                    PostContent = string.Empty,
                    CreatedAt = s.CreatedAt,
                    UserId = s.UserId,
                    FullName = s.User!.FullName ?? s.User.Username,
                    AvatarUrl = s.User.AvatarUrl,
                    ImageUrl = null,
                    Privacy = s.OriginalPost!.Privacy,
                    LikeCount = s.OriginalPost.Likes.Count(),
                    IsLiked = s.OriginalPost.Likes.Any(l => l.UserId == currentUserId),
                    CommentCount = s.OriginalPost.Comments.Count(),
                    GroupId = s.GroupId,
                    GroupName = s.Group != null ? s.Group.Name : null,
                    IsShare = true,
                    ShareId = s.ShareId,
                    ShareContent = s.Content,
                    OriginalPostId = s.PostId,
                    OriginalPost = new PostResponse
                    {
                        PostId = s.OriginalPost.PostId,
                        PostContent = s.OriginalPost.PostContent,
                        CreatedAt = s.OriginalPost.CreatedAt,
                        UserId = s.OriginalPost.UserId,
                        FullName = s.OriginalPost.User!.FullName ?? s.OriginalPost.User.Username,
                        AvatarUrl = s.OriginalPost.User.AvatarUrl,
                        ImageUrl = s.OriginalPost.ImageUrl,
                        Privacy = s.OriginalPost.Privacy,
                        IsSponsored = s.OriginalPost.IsSponsored,
                        SponsorEndDate = s.OriginalPost.SponsorEndDate
                    }
                })
                .ToListAsync();

            return posts.Concat(shares)
                .OrderByDescending(p => p.CreatedAt)
                .ToList();
        }

        // 3. Lấy bài viết của người khác
        public async Task<List<PostResponse>> GetPostsByUserIdAsync(Guid userId, Guid currentUserId)
        {
            var isFriend = await _context.FriendshipMembers
                .Where(fm => fm.UserId == currentUserId)
                .Join(_context.Friendships.Where(f => f.Status == "Accepted"),
                      fm => fm.FriendshipId,
                      f => f.FriendshipId,
                      (fm, f) => fm.FriendshipId)
                .AnyAsync(fid => _context.FriendshipMembers.Any(fm2 => fm2.FriendshipId == fid && fm2.UserId == userId));

            var posts = await _context.Posts
                .Where(p => p.UserId == userId)
                .Where(p => p.UserId == currentUserId 
                         || p.Privacy == "Public" 
                         || (p.Privacy == "Friends" && isFriend))
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
                    GroupId = p.GroupId,
                    GroupName = p.Group != null ? p.Group.Name : null,
                    IsShare = false,
                    IsSponsored = p.IsSponsored,
                    SponsorEndDate = p.SponsorEndDate
                })
                .ToListAsync();

            var shares = await _context.Shares
                .Where(s => s.UserId == userId)
                .Where(s => s.UserId == currentUserId 
                         || s.OriginalPost!.Privacy == "Public" 
                         || (s.OriginalPost.Privacy == "Friends" && isFriend))
                .Select(s => new PostResponse
                {
                    PostId = s.ShareId,
                    PostContent = string.Empty,
                    CreatedAt = s.CreatedAt,
                    UserId = s.UserId,
                    FullName = s.User!.FullName ?? s.User.Username,
                    AvatarUrl = s.User.AvatarUrl,
                    ImageUrl = null,
                    Privacy = s.OriginalPost!.Privacy,
                    LikeCount = s.OriginalPost.Likes.Count(),
                    IsLiked = s.OriginalPost.Likes.Any(l => l.UserId == currentUserId),
                    CommentCount = s.OriginalPost.Comments.Count(),
                    GroupId = s.GroupId,
                    GroupName = s.Group != null ? s.Group.Name : null,
                    IsShare = true,
                    ShareId = s.ShareId,
                    ShareContent = s.Content,
                    OriginalPostId = s.PostId,
                    OriginalPost = new PostResponse
                    {
                        PostId = s.OriginalPost.PostId,
                        PostContent = s.OriginalPost.PostContent,
                        CreatedAt = s.OriginalPost.CreatedAt,
                        UserId = s.OriginalPost.UserId,
                        FullName = s.OriginalPost.User!.FullName ?? s.OriginalPost.User.Username,
                        AvatarUrl = s.OriginalPost.User.AvatarUrl,
                        ImageUrl = s.OriginalPost.ImageUrl,
                        Privacy = s.OriginalPost.Privacy,
                        IsSponsored = s.OriginalPost.IsSponsored,
                        SponsorEndDate = s.OriginalPost.SponsorEndDate
                    }
                })
                .ToListAsync();

            return posts.Concat(shares)
                .OrderByDescending(p => p.CreatedAt)
                .ToList();
        }

        public async Task<List<PostResponse>> GetGroupPostsAsync(Guid groupId, Guid currentUserId)
        {
            var posts = await _context.Posts
                .Where(p => p.GroupId == groupId)
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
                    GroupId = p.GroupId,
                    GroupName = p.Group != null ? p.Group.Name : null,
                    IsShare = false,
                    IsSponsored = p.IsSponsored,
                    SponsorEndDate = p.SponsorEndDate
                })
                .ToListAsync();

            var shares = await _context.Shares
                .Where(s => s.GroupId == groupId)
                .Select(s => new PostResponse
                {
                    PostId = s.ShareId,
                    PostContent = string.Empty,
                    CreatedAt = s.CreatedAt,
                    UserId = s.UserId,
                    FullName = s.User!.FullName ?? s.User.Username,
                    AvatarUrl = s.User.AvatarUrl,
                    ImageUrl = null,
                    Privacy = s.OriginalPost!.Privacy,
                    LikeCount = s.OriginalPost.Likes.Count(),
                    IsLiked = s.OriginalPost.Likes.Any(l => l.UserId == currentUserId),
                    CommentCount = s.OriginalPost.Comments.Count(),
                    GroupId = s.GroupId,
                    GroupName = s.Group != null ? s.Group.Name : null,
                    IsShare = true,
                    ShareId = s.ShareId,
                    ShareContent = s.Content,
                    OriginalPostId = s.PostId,
                    OriginalPost = new PostResponse
                    {
                        PostId = s.OriginalPost.PostId,
                        PostContent = s.OriginalPost.PostContent,
                        CreatedAt = s.OriginalPost.CreatedAt,
                        UserId = s.OriginalPost.UserId,
                        FullName = s.OriginalPost.User!.FullName ?? s.OriginalPost.User.Username,
                        AvatarUrl = s.OriginalPost.User.AvatarUrl,
                        ImageUrl = s.OriginalPost.ImageUrl,
                        Privacy = s.OriginalPost.Privacy,
                        IsSponsored = s.OriginalPost.IsSponsored,
                        SponsorEndDate = s.OriginalPost.SponsorEndDate
                    }
                })
                .ToListAsync();

            return posts.Concat(shares)
                .OrderByDescending(p => p.CreatedAt)
                .ToList();
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
            var share = await _context.Shares.FindAsync(postId);
            if (share != null)
            {
                postId = share.PostId;
            }

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

        public async Task<bool> SharePostAsync(Share share)
        {
            _context.Shares.Add(share);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}