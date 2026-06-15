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

            // Lấy tất cả bài viết hợp lệ
            var postsQuery = await _context.Posts
                .Where(p => !p.IsViolated) // Ẩn các bài viết vi phạm chính sách khỏi bảng tin chung
                .Where(p => (p.GroupId == null || joinedGroupIds.Contains(p.GroupId.Value)) &&
                         (p.UserId == currentUserId 
                           || p.Privacy == "Public" 
                           || (p.Privacy == "Friends" && friendsIds.Contains(p.UserId))))
                .Select(p => new { 
                    Id = p.PostId, 
                    IsShare = false,
                    IsSponsored = p.IsSponsored && (p.SponsorEndDate == null || p.SponsorEndDate > DateTime.UtcNow)
                })
                .ToListAsync();

            // Lấy tất cả bài share hợp lệ
            var sharesQuery = await _context.Shares
                .Where(s => !s.OriginalPost!.IsViolated) // Ẩn các bài viết gốc bị vi phạm chính sách
                .Where(s => (s.GroupId == null || joinedGroupIds.Contains(s.GroupId.Value)) &&
                         (s.UserId == currentUserId 
                          || s.OriginalPost!.Privacy == "Public" 
                          || (s.OriginalPost.Privacy == "Friends" && friendsIds.Contains(s.UserId))))
                .Select(s => new { 
                    Id = s.ShareId, 
                    IsShare = true,
                    IsSponsored = false
                })
                .ToListAsync();

            var allItems = postsQuery.Concat(sharesQuery).ToList();

            if (!allItems.Any())
            {
                return new List<PostResponse>();
            }

            // Chia làm 2 nhóm: Bài quảng cáo và bài viết thường
            var sponsoredItems = allItems.Where(x => x.IsSponsored).ToList();
            var regularItems = allItems.Where(x => !x.IsSponsored).ToList();

            // Sáo trộn ngẫu nhiên cả 2 nhóm sử dụng thuật toán Fisher-Yates
            var rand = new Random();
            
            int nSpon = sponsoredItems.Count;
            while (nSpon > 1) {
                nSpon--;
                int k = rand.Next(nSpon + 1);
                var value = sponsoredItems[k];
                sponsoredItems[k] = sponsoredItems[nSpon];
                sponsoredItems[nSpon] = value;
            }

            int nReg = regularItems.Count;
            while (nReg > 1) {
                nReg--;
                int k = rand.Next(nReg + 1);
                var value = regularItems[k];
                regularItems[k] = regularItems[nReg];
                regularItems[nReg] = value;
            }

            // Trộn xen kẽ: 
            var blendedItems = new List<dynamic>();
            int sponsoredIndex = 0;
            int regularIndex = 0;

            // Quyết định ngẫu nhiên xem bài viết đầu tiên trên bảng tin là bài thường hay bài quảng cáo
            bool startWithRegular = rand.Next(0, 2) == 0; // 50% cơ hội bắt đầu bằng bài thường

            if (startWithRegular && regularIndex < regularItems.Count)
            {
                int firstBatch = rand.Next(3, 6); // Lấy trước 3 đến 5 bài thường lên đầu feed
                for (int i = 0; i < firstBatch && regularIndex < regularItems.Count; i++)
                {
                    blendedItems.Add(regularItems[regularIndex++]);
                }
            }

            while (sponsoredIndex < sponsoredItems.Count || regularIndex < regularItems.Count)
            {
                // Thêm 1 bài quảng cáo ngẫu nhiên nếu còn
                if (sponsoredIndex < sponsoredItems.Count)
                {
                    blendedItems.Add(sponsoredItems[sponsoredIndex++]);
                }

                // Thêm ngẫu nhiên khoảng 6-7 bài thường
                int regularCountToInsert = rand.Next(6, 8); // sinh ngẫu nhiên 6 hoặc 7
                for (int i = 0; i < regularCountToInsert && regularIndex < regularItems.Count; i++)
                {
                    blendedItems.Add(regularItems[regularIndex++]);
                }
            }

            // Áp dụng phân trang ở bộ nhớ (in-memory paging) sau khi đã sáo trộn và trộn tỷ lệ
            var paginatedItems = blendedItems
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            if (!paginatedItems.Any())
            {
                return new List<PostResponse>();
            }

            var postIds = paginatedItems.Where(x => !x.IsShare).Select(x => (Guid)x.Id).ToList();
            var shareIds = paginatedItems.Where(x => x.IsShare).Select(x => (Guid)x.Id).ToList();

            // 2. Chỉ query dữ liệu chi tiết cho đúng các items được chọn ở trang hiện tại
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
                        SponsorEndDate = p.SponsorEndDate,
                        IsViolated = p.IsViolated,
                        ViolationReason = p.ViolationReason,
                        IsAppealed = p.IsAppealed,
                        AppealReason = p.AppealReason
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
                            SponsorEndDate = s.OriginalPost.SponsorEndDate,
                            IsViolated = s.OriginalPost.IsViolated,
                            ViolationReason = s.OriginalPost.ViolationReason,
                            IsAppealed = s.OriginalPost.IsAppealed,
                            AppealReason = s.OriginalPost.AppealReason
                        }
                    })
                    .ToListAsync();
            }

            // Map lại về đúng vị trí đã sắp xếp ở trên
            var finalOrdered = new List<PostResponse>();
            foreach (var item in paginatedItems)
            {
                if (!item.IsShare)
                {
                    var p = posts.FirstOrDefault(x => x.PostId == item.Id);
                    if (p != null) finalOrdered.Add(p);
                }
                else
                {
                    var s = shares.FirstOrDefault(x => x.ShareId == item.Id);
                    if (s != null) finalOrdered.Add(s);
                }
            }

            return finalOrdered;
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
                    SponsorEndDate = p.SponsorEndDate,
                    IsViolated = p.IsViolated,
                    ViolationReason = p.ViolationReason,
                    IsAppealed = p.IsAppealed,
                    AppealReason = p.AppealReason
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
                        SponsorEndDate = s.OriginalPost.SponsorEndDate,
                        IsViolated = s.OriginalPost.IsViolated,
                        ViolationReason = s.OriginalPost.ViolationReason,
                        IsAppealed = s.OriginalPost.IsAppealed,
                        AppealReason = s.OriginalPost.AppealReason
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
                .Where(p => p.UserId == userId && !p.IsViolated) // Không lấy bài viết vi phạm khi người khác xem
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
                    SponsorEndDate = p.SponsorEndDate,
                    IsViolated = p.IsViolated,
                    ViolationReason = p.ViolationReason,
                    IsAppealed = p.IsAppealed,
                    AppealReason = p.AppealReason
                })
                .ToListAsync();

            var shares = await _context.Shares
                .Where(s => s.UserId == userId && !s.OriginalPost!.IsViolated) // Không lấy bài share vi phạm khi người khác xem
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
                        SponsorEndDate = s.OriginalPost.SponsorEndDate,
                        IsViolated = s.OriginalPost.IsViolated,
                        ViolationReason = s.OriginalPost.ViolationReason,
                        IsAppealed = s.OriginalPost.IsAppealed,
                        AppealReason = s.OriginalPost.AppealReason
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

        public async Task<PostResponse?> GetPostResponseByIdAsync(Guid id, Guid currentUserId)
        {
            return await _context.Posts
                .Include(p => p.User)
                .Include(p => p.Group)
                .Where(p => p.PostId == id)
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
                    SponsorEndDate = p.SponsorEndDate,
                    IsViolated = p.IsViolated,
                    ViolationReason = p.ViolationReason,
                    IsAppealed = p.IsAppealed,
                    AppealReason = p.AppealReason
                })
                .FirstOrDefaultAsync();
        }

        public async Task<bool> SharePostAsync(Share share)
        {
            _context.Shares.Add(share);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}