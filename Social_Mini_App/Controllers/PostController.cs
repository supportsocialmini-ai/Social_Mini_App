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

namespace Social_Mini_App.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PostController : ControllerBase
    {
        private readonly IPostService _postService;
        public PostController(IPostService postService) => _postService = postService;

        // 1. LẤY NEWSFEED
        [HttpGet]
        public async Task<IActionResult> GetNewsfeed()
        {
            var currentUserId = GetCurrentUserId();
            var posts = await _postService.GetNewsfeedAsync(currentUserId);
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
                CreatedAt = DateTime.UtcNow
            };

            if (uploadDto.ImageFile != null && uploadDto.ImageFile.Length > 0)
            {
                try
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
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

                    post.ImageUrl = $"/images/{fileName}";
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

        // 6. LẤY BÀI VIẾT CỦA NGƯỜI KHÁC
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserPosts(Guid userId)
        {
            var currentUserId = GetCurrentUserId();
            var posts = await _postService.GetPostsByUserIdAsync(userId, currentUserId);
            return Ok(ApiResponse<List<PostResponse>>.Ok(posts));
        }

        [HttpGet("{postId}/likes")]
        public async Task<IActionResult> GetPostLikes(Guid postId)
        {
            var likes = await _postService.GetPostLikesAsync(postId);
            return Ok(ApiResponse<List<UserSummaryDto>>.Ok(likes));
        }

        private Guid GetCurrentUserId()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (Guid.TryParse(userIdStr, out var userId)) return userId;
            return Guid.Empty;
        }
    }
}