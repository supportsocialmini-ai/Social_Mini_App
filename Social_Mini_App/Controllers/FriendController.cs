using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniSocialNetwork.Models;
using MiniSocialNetwork.Wrappers;
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
    public class FriendController : ControllerBase
    {
        private readonly IFriendService _friendService;
        public FriendController(IFriendService friendService) => _friendService = friendService;

        [HttpPost("request/{addresseeId}")]
        public async Task<IActionResult> SendRequest(Guid addresseeId)
        {
            var requesterId = GetCurrentUserId();
            if (await _friendService.SendFriendRequestAsync(requesterId, addresseeId))
                return Ok(ApiResponse<string>.Ok(FriendMsg.Request.Success));
                
            return BadRequest(ApiResponse<string>.Fail(FriendMsg.Request.Fail));
        }

        [HttpPost("accept/{requestId}")]
        public async Task<IActionResult> AcceptRequest(Guid requestId)
        {
            var userId = GetCurrentUserId();
            if (await _friendService.AcceptFriendRequestAsync(requestId, userId))
                return Ok(ApiResponse<string>.Ok(FriendMsg.Action.AcceptSuccess));
                
            return BadRequest(ApiResponse<string>.Fail(FriendMsg.Action.AcceptFail));
        }

        [HttpPost("decline/{requestId}")]
        public async Task<IActionResult> DeclineRequest(Guid requestId)
        {
            var userId = GetCurrentUserId();
            if (await _friendService.DeclineFriendRequestAsync(requestId, userId))
                return Ok(ApiResponse<string>.Ok(FriendMsg.Action.DeclineSuccess));
                
            return BadRequest(ApiResponse<string>.Fail(FriendMsg.Action.DeclineFail));
        }

        [HttpDelete("unfriend/{friendId}")]
        public async Task<IActionResult> Unfriend(Guid friendId)
        {
            var userId = GetCurrentUserId();
            if (await _friendService.UnfriendAsync(userId, friendId))
                return Ok(ApiResponse<string>.Ok(FriendMsg.Action.UnfriendSuccess));
                
            return BadRequest(ApiResponse<string>.Fail(FriendMsg.Action.UnfriendFail));
        }

        [HttpDelete("cancel/{requestId}")]
        public async Task<IActionResult> CancelRequest(Guid requestId)
        {
            var userId = GetCurrentUserId();
            if (await _friendService.CancelFriendRequestAsync(userId, requestId))
                return Ok(ApiResponse<string>.Ok(FriendMsg.Action.CancelSuccess));
                
            return BadRequest(ApiResponse<string>.Fail(FriendMsg.Action.CancelFail));
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetFriends()
        {
            var userId = GetCurrentUserId();
            var friends = await _friendService.GetFriendsAsync(userId);
            return Ok(ApiResponse<List<UserSummaryDto>>.Ok(friends));
        }

        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingRequests()
        {
            var userId = GetCurrentUserId();
            var requests = await _friendService.GetPendingRequestsAsync(userId);
            return Ok(ApiResponse<List<FriendRequestDto>>.Ok(requests));
        }

        [HttpGet("status/{otherUserId}")]
        public async Task<IActionResult> GetStatus(Guid otherUserId)
        {
            var userId = GetCurrentUserId();
            var result = await _friendService.GetFriendshipStatusAsync(userId, otherUserId);
            return Ok(ApiResponse<object>.Ok(new { status = result.Status, requestId = result.RequestId }));
        }

        private Guid GetCurrentUserId()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (Guid.TryParse(userIdStr, out var userId)) return userId;
            return Guid.Empty;
        }
    }
}
