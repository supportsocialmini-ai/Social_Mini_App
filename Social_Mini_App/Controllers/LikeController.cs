using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniSocialNetwork.Models;
using Social_Mini_App.Interfaces;
using System.Security.Claims;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class LikeController : ControllerBase
{
    private readonly ILikeService _likeService;
    public LikeController(ILikeService likeService) => _likeService = likeService;

    [HttpPost("{postId}")]
    public async Task<IActionResult> Toggle(Guid postId)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdStr == null || !Guid.TryParse(userIdStr, out var userId)) 
            return Unauthorized();
            
        var isLiked = await _likeService.ToggleLikeAsync(postId, userId);
        return Ok(new { isLiked });
    }
}