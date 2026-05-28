using Microsoft.EntityFrameworkCore;
using MiniSocialNetwork.Data;
using Social_Mini_App.Dtos.Responses;
using Social_Mini_App.Interfaces;
using Social_Mini_App.Models;

namespace Social_Mini_App.Services
{
    public class SearchService : ISearchService
    {
        private readonly DataContext _context;

        public SearchService(DataContext context)
        {
            _context = context;
        }

        public async Task<SearchResultResponse> SearchAsync(string query, Guid currentUserId, string? category = null)
        {
            var q = query.Trim().ToLower();

            // Tìm kiếm người dùng theo FullName hoặc Username
            var usersQuery = _context.Users
                .Where(u => u.IsActive && !u.IsDeleted &&
                    (u.FullName.ToLower().Contains(q) || u.Username.ToLower().Contains(q)));

            if (!string.IsNullOrWhiteSpace(category))
            {
                var categoryLower = category.Trim().ToLower();
                usersQuery = usersQuery.Where(u => u.Category != null && u.Category.Trim().ToLower() == categoryLower);
            }

            var users = await usersQuery
                .OrderBy(u => u.FullName)
                .Take(20)
                .Select(u => new UserSummaryDto
                {
                    UserId = u.UserId,
                    Username = u.Username,
                    FullName = u.FullName,
                    AvatarUrl = u.AvatarUrl
                })
                .ToListAsync();

            // Tìm kiếm nhóm Public theo tên, mô tả hoặc category
            var groupsQuery = _context.Groups
                .Include(g => g.Members)
                .Where(g => g.Privacy == "Public" &&
                    (g.Name.ToLower().Contains(q) || (g.Description != null && g.Description.ToLower().Contains(q))));

            if (!string.IsNullOrWhiteSpace(category))
            {
                var categoryLower = category.Trim().ToLower();
                groupsQuery = groupsQuery.Where(g => g.Category != null && g.Category.Trim().ToLower() == categoryLower);
            }

            var groups = await groupsQuery
                .OrderByDescending(g => g.Members.Count)
                .Take(20)
                .Select(g => new GroupSearchDto
                {
                    GroupId = g.GroupId,
                    Name = g.Name,
                    Description = g.Description,
                    AvatarUrl = g.AvatarUrl,
                    Privacy = g.Privacy,
                    Category = g.Category,
                    MemberCount = g.Members.Count(m => m.Status == "Active")
                })
                .ToListAsync();

            // Tìm kiếm bài viết Public theo nội dung
            var posts = await _context.Posts
                .Include(p => p.User)
                .Include(p => p.Likes)
                .Include(p => p.Comments)
                .Where(p => p.Privacy == "Public" && p.PostContent.ToLower().Contains(q))
                .OrderByDescending(p => p.CreatedAt)
                .Take(20)
                .Select(p => new PostSearchDto
                {
                    PostId = p.PostId,
                    PostContent = p.PostContent,
                    ImageUrl = p.ImageUrl,
                    AuthorId = p.UserId,
                    FullName = p.User!.FullName ?? p.User.Username,
                    AvatarUrl = p.User.AvatarUrl,
                    CreatedAt = p.CreatedAt,
                    LikeCount = p.Likes.Count(),
                    CommentCount = p.Comments.Count()
                })
                .ToListAsync();

            return new SearchResultResponse
            {
                Users = users,
                Posts = posts,
                Groups = groups
            };
        }
    }
}
