using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Social_Mini_App.Interfaces;
using Social_Mini_App.Dtos.Responses;
using Social_Mini_App.Dtos.Requests;
using Social_Mini_App.Models;
using MiniSocialNetwork.Wrappers;
using System.Security.Claims;
using Social_Mini_App.Messages;

namespace Social_Mini_App.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CommentController : ControllerBase
    {
        private readonly ICommentService _commentService;
        public CommentController(ICommentService commentService) => _commentService = commentService;

        [HttpGet("post/{postId}")]
        public async Task<IActionResult> GetByPost(Guid postId)
        {
            var comments = await _commentService.GetCommentsByPostIdAsync(postId);
            return Ok(ApiResponse<List<CommentResponse>>.Ok(comments));
        }

        [HttpPost]
        public async Task<IActionResult> Create(CommentRequest request)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdStr == null || !Guid.TryParse(userIdStr, out var userId))
                return Unauthorized(ApiResponse<CommentResponse>.Fail("Unauthorized"));

            var comment = new Comment
            {
                PostId = request.PostId,
                CommentContent = request.Content,
                ParentCommentId = request.ParentCommentId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _commentService.CreateCommentAsync(comment);

            if (result != null) return Ok(ApiResponse<CommentResponse>.Ok(result));
            return BadRequest(ApiResponse<CommentResponse>.Fail(CommentMsg.Upsert.CreateFail));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdStr == null || !Guid.TryParse(userIdStr, out var userId)) 
                return Unauthorized(ApiResponse<string>.Fail("Unauthorized"));

            if (await _commentService.DeleteCommentAsync(id, userId))
            {
                return Ok(ApiResponse<string>.Ok(CommentMsg.Delete.Success));
            }
            return BadRequest(ApiResponse<string>.Fail(CommentMsg.Delete.Fail));
        }
    }
}