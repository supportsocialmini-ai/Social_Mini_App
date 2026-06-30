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

            // 1. Tạo Query tổng quát lấy danh sách ID và thông tin cơ bản
            var postIdsQuery = _context.Posts
                .Where(p => !p.IsViolated && p.UserId != currentUserId)
                .Select(p => new {
                    Id = p.PostId,
                    IsShare = false,
                    IsSponsored = p.IsSponsored && (p.SponsorEndDate == null || p.SponsorEndDate > DateTime.UtcNow),
                    UserId = p.UserId,
                    GroupId = p.GroupId,
                    Privacy = p.Privacy,
                    CreatedAt = p.CreatedAt
                });

            var shareIdsQuery = _context.Shares
                .Where(s => !s.OriginalPost!.IsViolated && s.UserId != currentUserId)
                .Select(s => new {
                    Id = s.ShareId,
                    IsShare = true,
                    IsSponsored = false, // Share không tính là Sponsored
                    UserId = s.UserId,
                    GroupId = s.GroupId,
                    Privacy = s.OriginalPost!.Privacy,
                    CreatedAt = s.CreatedAt
                });

            var allIdsQuery = postIdsQuery.Concat(shareIdsQuery);

            // 2. Chạy 4 truy vấn tuần tự
            // - NHÓM 1: Bạn bè (Ưu tiên nhất, lấy 3 bài)
            var friendItems = await allIdsQuery
                .Where(x => friendsIds.Contains(x.UserId) && x.Privacy != "Private" && x.GroupId == null)
                .OrderByDescending(x => x.CreatedAt).ThenByDescending(x => x.Id)
                .Skip((page - 1) * 3).Take(3)
                .ToListAsync();

            // - NHÓM 2: Nhóm (Lấy 2 bài)
            var groupItems = await allIdsQuery
                .Where(x => x.GroupId != null && joinedGroupIds.Contains(x.GroupId.Value))
                .OrderByDescending(x => x.CreatedAt).ThenByDescending(x => x.Id)
                .Skip((page - 1) * 2).Take(2)
                .ToListAsync();

            // - NHÓM 3: Quảng cáo (Lấy 2 bài)
            var sponsorItems = await allIdsQuery
                .Where(x => x.IsSponsored)
                .OrderByDescending(x => x.CreatedAt).ThenByDescending(x => x.Id)
                .Skip((page - 1) * 2).Take(2)
                .ToListAsync();

            // Tính toán bù trừ nếu hụt bài ở 3 nhóm trên
            int missingFriends = 3 - friendItems.Count;
            int missingGroups = 2 - groupItems.Count;
            int missingSponsor = 2 - sponsorItems.Count;
            int exploreCount = 3 + missingFriends + missingGroups + missingSponsor;

            // - NHÓM 4: Bài Lạ (Explore, lấy bù để đủ 10 bài/trang)
            var exploreItems = await allIdsQuery
                .Where(x => !friendsIds.Contains(x.UserId) 
                            && x.UserId != currentUserId 
                            && x.GroupId == null 
                            && x.Privacy == "Public"
                            && !x.IsSponsored)
                .OrderByDescending(x => x.CreatedAt).ThenByDescending(x => x.Id)
                .Skip((page - 1) * 10).Take(exploreCount)
                .ToListAsync();

            // 3. Gộp bài Thường:
            // - Bạn bè: Giữ nguyên thứ tự thời gian mới nhất và đẩy lên đầu.
            // - Nhóm & Bài Lạ: Trộn ngẫu nhiên với nhau và xếp sau Bạn bè.
            var rand = new Random();
            var otherRegularItems = groupItems.Concat(exploreItems).OrderBy(x => rand.Next()).ToList();
            
            var regularItems = friendItems.Concat(otherRegularItems).ToList();

            // 4. Trộn Quảng cáo vào Bài thường
            var paginatedItems = new List<dynamic>();
            
            // 50% cơ hội Bài Quảng Cáo lên đầu trang
            bool startWithAd = sponsorItems.Any() && rand.Next(0, 2) == 0;
            
            int sponsorIdx = 0;
            int regularIdx = 0;

            if (startWithAd)
            {
                paginatedItems.Add(sponsorItems[sponsorIdx++]);
            }

            // Trộn các bài còn lại
            while (regularIdx < regularItems.Count || sponsorIdx < sponsorItems.Count)
            {
                // Bốc 2 đến 3 bài thường
                int takeRegular = rand.Next(2, 4);
                for (int i = 0; i < takeRegular && regularIdx < regularItems.Count; i++)
                {
                    paginatedItems.Add(regularItems[regularIdx++]);
                }

                // Cài cắm 1 bài quảng cáo
                if (sponsorIdx < sponsorItems.Count)
                {
                    paginatedItems.Add(sponsorItems[sponsorIdx++]);
                }
            }

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
                    if (p != null) {
                        p.IsFriend = friendsIds.Contains(p.UserId);
                        finalOrdered.Add(p);
                    }
                }
                else
                {
                    var s = shares.FirstOrDefault(x => x.ShareId == item.Id);
                    if (s != null) {
                        s.IsFriend = friendsIds.Contains(s.UserId);
                        finalOrdered.Add(s);
                    }
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

            var allItems = posts.Concat(shares)
                .OrderByDescending(p => p.CreatedAt)
                .ToList();

            foreach(var item in allItems) {
                item.IsFriend = false; // Bài của chính mình thì không hiện kết bạn
            }

            return allItems;
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

            var allItems = posts.Concat(shares)
                .OrderByDescending(p => p.CreatedAt)
                .ToList();

            foreach(var item in allItems) {
                item.IsFriend = isFriend;
            }

            return allItems;
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

            var friendsIds = await GetFriendsIdsAsync(currentUserId);
            var allItems = posts.Concat(shares)
                .OrderByDescending(p => p.CreatedAt)
                .ToList();

            foreach(var item in allItems) {
                item.IsFriend = friendsIds.Contains(item.UserId);
            }

            return allItems;
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