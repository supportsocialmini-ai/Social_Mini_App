using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniSocialNetwork.Wrappers;
using Social_Mini_App.Dtos;
using Social_Mini_App.Dtos.Requests;
using Social_Mini_App.Dtos.Responses;
using Social_Mini_App.Interfaces;
using Social_Mini_App.Models;
using System.Security.Claims;
using Social_Mini_App.Messages;

using MiniSocialNetwork.Data;
using Microsoft.EntityFrameworkCore;

namespace Social_Mini_App.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PostController : ControllerBase
    {
        private readonly IPostService _postService;
        private readonly DataContext _context;
        private readonly INotificationService _notifService;

        public PostController(IPostService postService, DataContext context, INotificationService notifService)
        {
            _postService = postService;
            _context = context;
            _notifService = notifService;
        }

        // 1. LẤY NEWSFEED
        [HttpGet]
        public async Task<IActionResult> GetNewsfeed([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var currentUserId = GetCurrentUserId();
            var posts = await _postService.GetNewsfeedAsync(currentUserId, page, pageSize);
            return Ok(ApiResponse<List<PostResponse>>.Ok(posts));
        }

        // 2. THÊM BÀI MỚI (Chỉ nội dung)
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PostUploadDto uploadDto)
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty) return Unauthorized(ApiResponse<string>.Fail("Unauthorized"));

            var post = new Post
            {
                UserId = userId,
                PostContent = uploadDto.Content,
                Privacy = uploadDto.Privacy,
                GroupId = uploadDto.GroupId,
                CreatedAt = DateTime.UtcNow
            };

            if (await _postService.CreatePostAsync(post))
                return Ok(ApiResponse<string>.Ok(PostMsg.Upsert.CreateSuccess));

            return BadRequest(ApiResponse<string>.Fail(PostMsg.Upsert.CreateFail));
        }

        // 2b. THÊM BÀI MỚI VỚI HÌNH ẢNH
        [HttpPost("/api/image-upload")]
        [AllowAnonymous]
        public async Task<IActionResult> CreateWithImage([FromForm] PostUploadDto uploadDto)
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty) return Unauthorized(ApiResponse<string>.Fail("Unauthorized"));

            var post = new Post
            {
                UserId = userId,
                PostContent = uploadDto.Content,
                Privacy = uploadDto.Privacy,
                GroupId = uploadDto.GroupId,
                CreatedAt = DateTime.UtcNow
            };

            if (uploadDto.ImageFile != null && uploadDto.ImageFile.Length > 0)
            {
                try
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "posts");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var fileName = $"{Guid.NewGuid()}_{uploadDto.ImageFile.FileName}";
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await uploadDto.ImageFile.CopyToAsync(stream);
                    }

                    post.ImageUrl = $"/images/posts/{fileName}";
                }
                catch (Exception ex)
                {
                    return BadRequest(ApiResponse<string>.Fail(PostMsg.Upsert.ImageUploadFail, ex.Message));
                }
            }

            if (await _postService.CreatePostAsync(post)) 
                return Ok(ApiResponse<string>.Ok(PostMsg.Upsert.CreateSuccess));
                
            return BadRequest(ApiResponse<string>.Fail(PostMsg.Upsert.CreateFail));
        }

        // 2c. SHARE BÀI VIẾT
        [HttpPost("{id}/share")]
        public async Task<IActionResult> Share(Guid id, [FromBody] ShareRequest shareDto)
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty) return Unauthorized(ApiResponse<string>.Fail("Unauthorized"));

            var originalPost = await _postService.GetPostByIdAsync(id);
            if (originalPost == null) 
                return NotFound(ApiResponse<string>.Fail(PostMsg.Get.NotFound));

            var share = new Share
            {
                UserId = userId,
                PostId = id,
                Content = shareDto.Content,
                GroupId = shareDto.GroupId,
                CreatedAt = DateTime.UtcNow
            };

            if (await _postService.SharePostAsync(share))
                return Ok(ApiResponse<string>.Ok(PostMsg.Upsert.CreateSuccess));

            return BadRequest(ApiResponse<string>.Fail(PostMsg.Upsert.CreateFail));
        }

        // 3. SỬA BÀI
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] PostUpdateRequest request)
        {
            var userId = GetCurrentUserId();
            var postInDb = await _postService.GetPostByIdAsync(id);

            if (postInDb == null)
                return NotFound(ApiResponse<string>.Fail(PostMsg.Get.NotFound));

            if (postInDb.UserId != userId)
                return Forbid();

            postInDb.PostContent = request.Content;
            postInDb.Privacy = request.Privacy;

            if (await _postService.UpdatePostAsync(postInDb))
                return Ok(ApiResponse<string>.Ok(PostMsg.Upsert.UpdateSuccess));

            return BadRequest(ApiResponse<string>.Fail(PostMsg.Upsert.UpdateFail));
        }

        // 4. XÓA BÀI
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var userId = GetCurrentUserId();
            var post = await _postService.GetPostByIdAsync(id);

            if (post == null) 
                return NotFound(ApiResponse<string>.Fail("Post not found"));
                
            if (post.UserId != userId) 
                return Forbid();

            if (await _postService.DeletePostAsync(id)) 
                return Ok(ApiResponse<string>.Ok(PostMsg.Delete.Success));
                
            return BadRequest(ApiResponse<string>.Fail(PostMsg.Delete.Fail));
        }

        // 5. LẤY BÀI VIẾT CỦA CHÍNH TÔI
        [HttpGet("MyPost")]
        public async Task<IActionResult> GetMyPosts()
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty) 
                return Unauthorized(ApiResponse<List<PostResponse>>.Fail("Unauthorized"));

            var posts = await _postService.GetMyPostsAsync(userId, userId);
            return Ok(ApiResponse<List<PostResponse>>.Ok(posts));
        }

        // 3. LẤY CHI TIẾT BÀI VIẾT THEO ID (hỗ trợ trang kháng nghị)
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPostById(Guid id)
        {
            var currentUserId = GetCurrentUserId();
            var post = await _postService.GetPostResponseByIdAsync(id, currentUserId);
            if (post == null)
                return NotFound(ApiResponse<PostResponse>.Fail("Không tìm thấy bài viết"));

            // Nếu bài viết bị đánh dấu vi phạm, chỉ cho phép tác giả bài viết hoặc Admin xem
            var isAdmin = User.IsInRole("Admin");
            if (post.IsViolated && post.UserId != currentUserId && !isAdmin)
            {
                return Forbid();
            }

            return Ok(ApiResponse<PostResponse>.Ok(post));
        }

        // 6. LẤY BÀI VIẾT CỦA NGƯỜI KHÁC
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserPosts(Guid userId)
        {
            var currentUserId = GetCurrentUserId();
            var posts = await _postService.GetPostsByUserIdAsync(userId, currentUserId);
            return Ok(ApiResponse<List<PostResponse>>.Ok(posts));
        }

        // 7. LẤY BÀI VIẾT CỦA NHÓM
        [HttpGet("group/{groupId}")]
        public async Task<IActionResult> GetGroupPosts(Guid groupId)
        {
            var currentUserId = GetCurrentUserId();
            var posts = await _postService.GetGroupPostsAsync(groupId, currentUserId);
            return Ok(ApiResponse<List<PostResponse>>.Ok(posts));
        }

        [HttpGet("{postId}/likes")]
        public async Task<IActionResult> GetPostLikes(Guid postId)
        {
            var likes = await _postService.GetPostLikesAsync(postId);
            return Ok(ApiResponse<List<UserSummaryDto>>.Ok(likes));
        }

        [HttpPost("{id}/appeal")]
        public async Task<IActionResult> AppealPost(Guid id, [FromBody] AppealRequestDto dto)
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty) return Unauthorized(ApiResponse<string>.Fail("Unauthorized"));

            var post = await _postService.GetPostByIdAsync(id);
            if (post == null) return NotFound(ApiResponse<string>.Fail("Không tìm thấy bài viết"));

            if (post.UserId != userId) return Forbid();
            if (!post.IsViolated) return BadRequest(ApiResponse<string>.Fail("Bài viết không ở trạng thái vi phạm để kháng nghị."));

            post.IsAppealed = true;
            post.AppealReason = dto.Reason;
            post.UpdatedAt = DateTime.UtcNow;

            if (await _postService.UpdatePostAsync(post))
            {
                // 1. Chuyển đổi trạng thái của tất cả báo cáo liên quan về lại Pending
                var reports = await _context.Reports
                    .Where(r => r.TargetType == "Post" && r.TargetId == id)
                    .ToListAsync();
                foreach (var r in reports)
                {
                    r.Status = "Pending";
                    r.ResolvedAt = null;
                    r.ResolvedById = null;
                }
                await _context.SaveChangesAsync();

                // 2. Gửi thông báo cho toàn bộ Admin
                var userObj = await _context.Users.FindAsync(userId);
                string displayName = userObj?.FullName ?? userObj?.Username ?? "Thành viên";

                var admins = await _context.Users
                    .Where(u => u.UserRoles.Any(ur => ur.Role.Name == "Admin"))
                    .ToListAsync();
                foreach (var admin in admins)
                {
                    await _notifService.CreateNotifAsync(
                        userId,
                        admin.UserId,
                        post.PostId,
                        "AppealSubmit",
                        $"{displayName} đã gửi kháng nghị bài viết"
                    );
                }

                return Ok(ApiResponse<string>.Ok("Đã gửi kháng nghị của bạn đến ban quản trị thành công. Vui lòng chờ phê duyệt."));
            }

            return BadRequest(ApiResponse<string>.Fail("Không thể gửi yêu cầu kháng nghị lúc này."));
        }

        [HttpPost("{id}/toggle-sponsor")]
        public async Task<IActionResult> ToggleSponsor(Guid id)
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty) return Unauthorized(ApiResponse<string>.Fail("Unauthorized"));

            var post = await _postService.GetPostByIdAsync(id);
            if (post == null) return NotFound(ApiResponse<string>.Fail("Không tìm thấy bài viết"));

            if (post.UserId != userId) return Forbid();

            // Kiểm tra xem user có gói cước nào sở hữu tính năng quảng cáo bài viết (ví dụ: "Sponsor Post" hoặc "Premium Ads")
            var userService = HttpContext.RequestServices.GetRequiredService<IUserService>();
            var user = await userService.GetUserByIdAsync(userId);
            
            bool hasSponsorFeature = user != null && (
                user.ActiveFeatures.Contains("Sponsor Post") || 
                user.ActiveFeatures.Contains("Premium Ads") || 
                user.Subscriptions.Any(s => s.IsActive && s.Package != null && 
                    (s.Package.Features != null && (s.Package.Features.Contains("Sponsor Post") || s.Package.Features.Contains("Premium Ads"))))
            );

            // Cho phép Admin luôn có quyền quảng cáo
            bool isAdmin = User.IsInRole("Admin");

            if (!post.IsSponsored && !hasSponsorFeature && !isAdmin)
            {
                return BadRequest(ApiResponse<string>.Fail("Bạn cần đăng ký gói dịch vụ hỗ trợ Quảng cáo bài viết để sử dụng tính năng này!"));
            }

            post.IsSponsored = !post.IsSponsored;
            if (await _postService.UpdatePostAsync(post))
            {
                return Ok(ApiResponse<bool>.Ok(post.IsSponsored));
            }

            return BadRequest(ApiResponse<string>.Fail("Không thể thực hiện yêu cầu lúc này, vui lòng thử lại sau."));
        }

        private Guid GetCurrentUserId()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (Guid.TryParse(userIdStr, out var userId)) return userId;
            return Guid.Empty;
        }
    }
}