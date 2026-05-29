using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniSocialNetwork.Data;
using MiniSocialNetwork.Models;
using MiniSocialNetwork.Wrappers;
using Social_Mini_App.Interfaces;
using Social_Mini_App.Models;
using Social_Mini_App.Dtos;

namespace Social_Mini_App.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IUserService _userService;

        public AdminController(DataContext context, IUserService _userService)
        {
            _context = context;
            this._userService = _userService;
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var totalUsers = await _context.Users.CountAsync();
            var totalPosts = await _context.Posts.CountAsync();
            var totalComments = await _context.Comments.CountAsync() + await _context.Replies.CountAsync();
            var activeUsers = await _context.Users.CountAsync(u => u.IsActive);

            return Ok(ApiResponse<object>.Ok(new
            {
                TotalUsers = totalUsers,
                TotalPosts = totalPosts,
                TotalComments = totalComments,
                ActiveUsers = activeUsers
            }));
        }

        [AllowAnonymous]
        [HttpGet("maintenance-status")]
        public async Task<ActionResult<object>> GetMaintenanceStatus()
        {
            var setting = await _context.SystemSettings.FirstOrDefaultAsync(s => s.Key == "MaintenanceMode");
            return Ok(new { isMaintenance = setting?.Value?.ToLower() == "true" });
        }

        [HttpPost("toggle-maintenance")]
        public async Task<ActionResult<object>> ToggleMaintenance()
        {
            var setting = await _context.SystemSettings.FirstOrDefaultAsync(s => s.Key == "MaintenanceMode");
            if (setting == null) return NotFound();

            bool currentStatus = setting.Value.ToLower() == "true";
            setting.Value = (!currentStatus).ToString().ToLower();
            setting.LastModified = DateTime.Now;

            await _context.SaveChangesAsync();
            return Ok(new { isMaintenance = setting.Value == "true" });
        }

        [AllowAnonymous]
        [HttpGet("maintenance-info")]
        public async Task<ActionResult<object>> GetMaintenanceInfo()
        {
            var settings = await _context.SystemSettings
                .Where(s => s.Key == "MaintenanceMode" || s.Key == "MaintenanceReason" || s.Key == "MaintenanceVersion" || s.Key == "MaintenanceEndTime")
                .ToListAsync();

            bool isMaintenance = settings.FirstOrDefault(s => s.Key == "MaintenanceMode")?.Value?.ToLower() == "true";
            string reason = settings.FirstOrDefault(s => s.Key == "MaintenanceReason")?.Value ?? "";
            string version = settings.FirstOrDefault(s => s.Key == "MaintenanceVersion")?.Value ?? "";
            string endTime = settings.FirstOrDefault(s => s.Key == "MaintenanceEndTime")?.Value ?? "";

            return Ok(new { isMaintenance, reason, version, endTime });
        }

        [HttpPost("maintenance-info")]
        public async Task<ActionResult<object>> SaveMaintenanceInfo([FromBody] MaintenanceInfoDto dto)
        {
            var keys = new[] { "MaintenanceReason", "MaintenanceVersion", "MaintenanceEndTime" };
            var values = new[] { dto.Reason ?? "", dto.Version ?? "", dto.EndTime ?? "" };

            for (int i = 0; i < keys.Length; i++)
            {
                var setting = await _context.SystemSettings.FirstOrDefaultAsync(s => s.Key == keys[i]);
                if (setting == null)
                {
                    _context.SystemSettings.Add(new Social_Mini_App.Models.SystemSetting
                    {
                        Key = keys[i],
                        Value = values[i],
                        Description = $"Maintenance {keys[i]}",
                        LastModified = DateTime.Now
                    });
                }
                else
                {
                    setting.Value = values[i];
                    setting.LastModified = DateTime.Now;
                }
            }

            await _context.SaveChangesAsync();
            return Ok(new { success = true, reason = dto.Reason, version = dto.Version, endTime = dto.EndTime });
        }

        [HttpPost("packages")]
        public async Task<IActionResult> CreatePackage([FromBody] SubscriptionPackage package)
        {
            package.Id = Guid.NewGuid();
            package.CreatedAt = DateTime.Now;
            _context.SubscriptionPackages.Add(package);
            await _context.SaveChangesAsync();
            return Ok(ApiResponse<SubscriptionPackage>.Ok(package));
        }

        [HttpGet("packages")]
        public async Task<IActionResult> GetPackages()
        {
            var packages = await _context.SubscriptionPackages.ToListAsync();
            return Ok(ApiResponse<object>.Ok(packages));
        }

        [HttpPut("packages/{id}")]
        public async Task<IActionResult> UpdatePackage(Guid id, [FromBody] UpdatePackageRequest request)
        {
            var package = await _context.SubscriptionPackages.FindAsync(id);
            if (package == null) return NotFound(ApiResponse<string>.Fail("Không tìm thấy gói dịch vụ"));

            package.Name = request.Name;
            package.Price = request.Price;
            package.IsActive = request.IsActive;
            package.Description = request.Description;
            package.Features = request.Features;
            package.DurationDays = request.DurationDays;
            package.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return Ok(ApiResponse<string>.Ok("Cập nhật gói dịch vụ thành công"));
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? searchTerm = null)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            var query = _context.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(u => u.FullName.ToLower().Contains(term) || u.Username.ToLower().Contains(term) || u.Email.ToLower().Contains(term));
            }

            var totalUsers = await query.CountAsync();

            var users = await query
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .OrderByDescending(u => u.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new
                {
                    u.UserId,
                    u.Username,
                    u.FullName,
                    u.Email,
                    u.AvatarUrl,
                    u.IsActive,
                    u.IsVerified,
                    u.CreatedAt,
                    Roles = u.UserRoles.Select(ur => ur.Role.Name).ToList()
                })
                .ToListAsync();

            return Ok(ApiResponse<object>.Ok(new {
                totalCount = totalUsers,
                page = page,
                pageSize = pageSize,
                users = users
            }));
        }

        [HttpPost("users/{userId}/toggle-status")]
        public async Task<IActionResult> ToggleUserStatus(Guid userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound(ApiResponse<string>.Fail("Không tìm thấy người dùng"));

            user.IsActive = !user.IsActive;
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<bool>.Ok(user.IsActive));
        }

        [HttpDelete("posts/{postId}")]
        public async Task<IActionResult> DeletePostByAdmin(Guid postId)
        {
            var post = await _context.Posts.FindAsync(postId);
            if (post == null) return NotFound(ApiResponse<string>.Fail("Không tìm thấy bài viết"));

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<string>.Ok("Đã xóa bài viết bởi Admin"));
        }

        [HttpGet("groups")]
        public async Task<IActionResult> GetAllGroups()
        {
            var groups = await _context.Groups
                .Include(g => g.Creator)
                .Include(g => g.Members)
                .OrderByDescending(g => g.CreatedAt)
                .Select(g => new
                {
                    GroupId = g.GroupId,
                    Name = g.Name,
                    Description = g.Description,
                    AvatarUrl = g.AvatarUrl,
                    CoverUrl = g.CoverUrl,
                    Privacy = g.Privacy,
                    Category = g.Category,
                    CreatedAt = g.CreatedAt,
                    CreatedBy = g.CreatedBy,
                    CreatorName = g.Creator != null ? g.Creator.FullName : "Không xác định",
                    CreatorUsername = g.Creator != null ? g.Creator.Username : "unknown",
                    MemberCount = g.Members.Count(m => m.Status == "Active")
                })
                .ToListAsync();

            return Ok(ApiResponse<object>.Ok(groups));
        }

        [HttpDelete("groups/{groupId}")]
        public async Task<IActionResult> DeleteGroup(Guid groupId)
        {
            var group = await _context.Groups
                .Include(g => g.Conversation)
                .FirstOrDefaultAsync(g => g.GroupId == groupId);

            if (group == null)
                return NotFound(ApiResponse<string>.Fail("Không tìm thấy nhóm"));

            // 1. Xóa các bài viết thuộc nhóm
            var posts = await _context.Posts
                .Where(p => p.GroupId == groupId)
                .ToListAsync();
            _context.Posts.RemoveRange(posts);

            // 2. Xóa cuộc trò chuyện và tin nhắn
            if (group.Conversation != null)
            {
                var participants = await _context.ConversationParticipants
                    .Where(cp => cp.ConversationId == group.Conversation.ConversationId)
                    .ToListAsync();
                _context.ConversationParticipants.RemoveRange(participants);

                var messages = await _context.Messages
                    .Where(m => m.ConversationId == group.Conversation.ConversationId)
                    .ToListAsync();
                _context.Messages.RemoveRange(messages);

                _context.Conversations.Remove(group.Conversation);
            }

            // 3. Xóa các thành viên nhóm
            var members = await _context.GroupMembers
                .Where(gm => gm.GroupId == groupId)
                .ToListAsync();
            _context.GroupMembers.RemoveRange(members);

            // 4. Xóa chính nhóm đó
            _context.Groups.Remove(group);

            await _context.SaveChangesAsync();
            return Ok(ApiResponse<string>.Ok("Đã xóa nhóm thành công"));
        }

        [HttpGet("detailed-stats")]
        public async Task<IActionResult> GetDetailedStats([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            var start = startDate ?? DateTime.Now.AddDays(-30);
            var end = endDate ?? DateTime.Now;

            start = start.Date;
            end = end.Date.AddDays(1).AddTicks(-1);

            // 1. Get raw records in date range
            var usersList = await _context.Users
                .Where(u => u.CreatedAt >= start && u.CreatedAt <= end)
                .Select(u => new { u.UserId, u.Username, u.FullName, u.Email, u.CreatedAt })
                .ToListAsync();

            var postsList = await _context.Posts
                .Where(p => p.CreatedAt >= start && p.CreatedAt <= end)
                .Select(p => new { p.PostId, p.PostContent, p.CreatedAt, p.UserId })
                .ToListAsync();

            var premiumSubsList = await _context.Subscriptions
                .Include(s => s.Package)
                .Where(s => s.CreatedAt >= start && s.CreatedAt <= end)
                .Select(s => new { s.Id, s.UserId, s.Tier, PackageName = s.Package != null ? s.Package.Name : s.Tier, s.CreatedAt })
                .ToListAsync();

            var adsList = await _context.Payments
                .Include(p => p.Package)
                .Include(p => p.User)
                .Where(p => p.PostId != null && p.Status == "Success" && p.CreatedAt >= start && p.CreatedAt <= end)
                .Select(p => new
                {
                    p.Id,
                    p.UserId,
                    UserFullName = p.User != null ? p.User.FullName : "Không xác định",
                    UserUsername = p.User != null ? p.User.Username : "unknown",
                    UserEmail = p.User != null ? p.User.Email : "",
                    p.PostId,
                    PackageName = p.Package != null ? p.Package.Name : "Quảng cáo",
                    p.Amount,
                    p.CreatedAt
                })
                .ToListAsync();

            var reportsList = await (
                from r in _context.Reports
                join p in _context.Posts on r.TargetId equals p.PostId
                join u in _context.Users on p.UserId equals u.UserId
                where r.TargetType == "Post" && r.CreatedAt >= start && r.CreatedAt <= end
                select new
                {
                    r.ReportId,
                    r.TargetId,
                    r.ReporterId,
                    PostAuthorId = p.UserId,
                    PostAuthorName = u.FullName,
                    PostAuthorUsername = u.Username,
                    PostAuthorEmail = u.Email,
                    r.Reason,
                    r.Description,
                    r.CreatedAt
                }
            ).ToListAsync();

            // 2. Aggregate counts by date (Day-Month-Year)
            var dailyData = new List<object>();
            for (var date = start.Date; date <= end.Date; date = date.AddDays(1))
            {
                var dayStart = date;
                var dayEnd = date.AddDays(1).AddTicks(-1);

                var joined = usersList.Count(u => u.CreatedAt >= dayStart && u.CreatedAt <= dayEnd);
                var posts = postsList.Count(p => p.CreatedAt >= dayStart && p.CreatedAt <= dayEnd);
                var premiums = premiumSubsList.Count(s => s.CreatedAt >= dayStart && s.CreatedAt <= dayEnd);
                var reported = reportsList.Count(r => r.CreatedAt >= dayStart && r.CreatedAt <= dayEnd);
                var ads = adsList.Count(a => a.CreatedAt >= dayStart && a.CreatedAt <= dayEnd);

                dailyData.Add(new
                {
                    Date = date.ToString("yyyy-MM-dd"),
                    JoinedUsers = joined,
                    CreatedPosts = posts,
                    PremiumRegistrations = premiums,
                    AdsRegistrations = ads,
                    ReportedPosts = reported
                });
            }

            // 3. Detailed events list
            var events = new List<DetailedEventDto>();

            // Get other users involved to load their details (creators/subscribers)
            var userIdsToFetch = postsList.Select(p => p.UserId)
                .Concat(premiumSubsList.Select(s => s.UserId))
                .Concat(reportsList.Select(r => r.ReporterId))
                .Distinct()
                .ToList();

            // Add ads events
            foreach (var a in adsList)
            {
                events.Add(new DetailedEventDto
                {
                    Type = "Ads",
                    TypeName = "Đăng ký quảng cáo",
                    UserId = a.UserId,
                    FullName = a.UserFullName,
                    Username = a.UserUsername,
                    Email = a.UserEmail,
                    Time = a.CreatedAt,
                    Details = $"Mua quảng cáo cho bài viết (ID: {a.PostId}) - Gói: {a.PackageName} - {a.Amount:N0} VNĐ"
                });
            }

            var usersMap = await _context.Users
                .Where(u => userIdsToFetch.Contains(u.UserId))
                .ToDictionaryAsync(u => u.UserId, u => new { u.FullName, u.Username, u.Email });

            // Add registers
            foreach (var u in usersList)
            {
                events.Add(new DetailedEventDto
                {
                    Type = "Register",
                    TypeName = "Đăng ký tài khoản",
                    UserId = u.UserId,
                    FullName = u.FullName,
                    Username = u.Username,
                    Email = u.Email,
                    Time = u.CreatedAt,
                    Details = "Đăng ký tài khoản mới"
                });
            }

            // Add posts
            foreach (var p in postsList)
            {
                usersMap.TryGetValue(p.UserId, out var creator);
                events.Add(new DetailedEventDto
                {
                    Type = "Post",
                    TypeName = "Đăng bài viết",
                    UserId = p.UserId,
                    FullName = creator?.FullName ?? "Không xác định",
                    Username = creator?.Username ?? "unknown",
                    Email = creator?.Email ?? "",
                    Time = p.CreatedAt,
                    Details = p.PostContent.Length > 100 ? p.PostContent.Substring(0, 100) + "..." : p.PostContent
                });
            }

            // Add premium subs
            foreach (var s in premiumSubsList)
            {
                usersMap.TryGetValue(s.UserId, out var subscriber);
                events.Add(new DetailedEventDto
                {
                    Type = "Premium",
                    TypeName = "Đăng ký Premium",
                    UserId = s.UserId,
                    FullName = subscriber?.FullName ?? "Không xác định",
                    Username = subscriber?.Username ?? "unknown",
                    Email = subscriber?.Email ?? "",
                    Time = s.CreatedAt,
                    Details = $"Đăng ký gói nâng cấp: {s.PackageName}"
                });
            }

            // Add reports
            foreach (var r in reportsList)
            {
                events.Add(new DetailedEventDto
                {
                    Type = "Report",
                    TypeName = "Bài viết bị báo cáo",
                    UserId = r.PostAuthorId,
                    FullName = r.PostAuthorName,
                    Username = r.PostAuthorUsername,
                    Email = r.PostAuthorEmail,
                    Time = r.CreatedAt,
                    Details = $"Bài viết (ID: {r.TargetId}) bị báo cáo. Lý do: {r.Reason}. Chi tiết: {r.Description}"
                });
            }

            // Sort events by Time descending
            var sortedEvents = events
                .OrderByDescending(e => e.Time)
                .Select(e => new
                {
                    e.Type,
                    e.TypeName,
                    e.UserId,
                    e.FullName,
                    e.Username,
                    e.Email,
                    Time = e.Time.ToString("yyyy-MM-dd HH:mm:ss"),
                    e.Details
                })
                .ToList();

            return Ok(ApiResponse<object>.Ok(new
            {
                DailyStats = dailyData,
                Events = sortedEvents,
                TotalJoined = usersList.Count,
                TotalPosts = postsList.Count,
                TotalPremiums = premiumSubsList.Count,
                TotalAds = adsList.Count,
                TotalReports = reportsList.Count
            }));
        }

        [HttpGet("revenue")]
        public async Task<IActionResult> GetRevenue([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            var start = (startDate ?? DateTime.Now.AddDays(-30)).Date;
            var end = (endDate ?? DateTime.Now).Date.AddDays(1).AddTicks(-1);

            // 1. Lấy tất cả payments thành công
            var successPayments = await _context.Payments
                .Include(p => p.Package)
                .Include(p => p.User)
                .Where(p => p.Status == "Success" && p.CreatedAt >= start && p.CreatedAt <= end)
                .ToListAsync();

            var premiumPayments = successPayments.Where(p => p.PostId == null).ToList();
            var adsPayments = successPayments.Where(p => p.PostId != null).ToList();

            // 2. Tổng doanh thu
            decimal totalRevenue = successPayments.Sum(p => p.Amount);
            decimal premiumRevenue = premiumPayments.Sum(p => p.Amount);
            decimal adsRevenue = adsPayments.Sum(p => p.Amount);

            // 3. Doanh thu theo ngày
            var dailyRevenue = new List<object>();
            for (var date = start.Date; date <= end.Date; date = date.AddDays(1))
            {
                var dayStart = date;
                var dayEnd = date.AddDays(1).AddTicks(-1);
                var dayPremium = premiumPayments.Where(p => p.CreatedAt >= dayStart && p.CreatedAt <= dayEnd).Sum(p => p.Amount);
                var dayAds = adsPayments.Where(p => p.CreatedAt >= dayStart && p.CreatedAt <= dayEnd).Sum(p => p.Amount);
                dailyRevenue.Add(new
                {
                    Date = date.ToString("yyyy-MM-dd"),
                    PremiumRevenue = dayPremium,
                    AdsRevenue = dayAds,
                    TotalRevenue = dayPremium + dayAds
                });
            }

            // 4. Top gói bán chạy
            var packageSales = successPayments
                .GroupBy(p => new { Name = p.Package != null ? p.Package.Name : (p.PostId != null ? "Quảng cáo" : "Không xác định"), IsAds = p.PostId != null })
                .Select(g => new
                {
                    PackageName = g.Key.Name,
                    IsAds = g.Key.IsAds,
                    Count = g.Count(),
                    TotalAmount = g.Sum(p => p.Amount)
                })
                .OrderByDescending(x => x.TotalAmount)
                .ToList();

            // 5. Danh sách giao dịch chi tiết (50 giao dịch mới nhất)
            var transactions = successPayments
                .OrderByDescending(p => p.CreatedAt)
                .Take(100)
                .Select(p => new
                {
                    Id = p.Id,
                    UserFullName = p.User != null ? p.User.FullName : "Không xác định",
                    UserUsername = p.User != null ? p.User.Username : "unknown",
                    UserEmail = p.User != null ? p.User.Email : "",
                    PackageName = p.Package != null ? p.Package.Name : (p.PostId != null ? "Quảng cáo bài viết" : "Không xác định"),
                    Type = p.PostId != null ? "Ads" : "Premium",
                    Amount = p.Amount,
                    PostId = p.PostId,
                    CreatedAt = p.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")
                })
                .ToList();

            return Ok(ApiResponse<object>.Ok(new
            {
                TotalRevenue = totalRevenue,
                PremiumRevenue = premiumRevenue,
                AdsRevenue = adsRevenue,
                TotalTransactions = successPayments.Count,
                PremiumCount = premiumPayments.Count,
                AdsCount = adsPayments.Count,
                DailyRevenue = dailyRevenue,
                PackageSales = packageSales,
                Transactions = transactions
            }));
        }
    }

    public class DetailedEventDto
    {
        public string Type { get; set; } = string.Empty;
        public string TypeName { get; set; } = string.Empty;
        public Guid UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime Time { get; set; }
        public string Details { get; set; } = string.Empty;
    }

    public class UpdatePackageRequest
    {
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public bool IsActive { get; set; }
        public string? Description { get; set; }
        public string? Features { get; set; }
        public int DurationDays { get; set; }
    }
}
