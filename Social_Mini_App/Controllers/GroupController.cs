using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniSocialNetwork.Interfaces;
using MiniSocialNetwork.Wrappers;
using Social_Mini_App.Dtos.Requests;
using Social_Mini_App.Messages;
using Social_Mini_App.Models;
using System.Security.Claims;
using User = MiniSocialNetwork.Models.User;

namespace Social_Mini_App.Controllers
{
    [Authorize]
    [Route("api/group")]
    [ApiController]
    public class GroupController : ControllerBase
    {
        private readonly IGroupService _groupService;

        public GroupController(IGroupService groupService)
        {
            _groupService = groupService;
        }

        private Guid GetCurrentUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpPost("create")]
        public async Task<IActionResult> CreateGroup([FromBody] CreateGroupRequest request)
        {
            var userId = GetCurrentUserId();
            var group = await _groupService.CreateGroupAsync(userId, request);
            if (group != null)
                return Ok(ApiResponse<Group>.Ok(group));
            
            return BadRequest(ApiResponse<string>.Fail(GroupMsg.Upsert.CreateFail));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetGroup(Guid id)
        {
            var group = await _groupService.GetGroupByIdAsync(id);
            if (group == null) 
                return NotFound(ApiResponse<string>.Fail(GroupMsg.Get.NotFound));
            
            return Ok(ApiResponse<Group>.Ok(group));
        }

        [HttpGet("my-groups")]
        public async Task<IActionResult> GetMyGroups()
        {
            var userId = GetCurrentUserId();
            var groups = await _groupService.GetUserGroupsAsync(userId);
            return Ok(ApiResponse<IEnumerable<Group>>.Ok(groups));
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchGroups([FromQuery] string? query)
        {
            var groups = await _groupService.SearchGroupsAsync(query ?? string.Empty);
            return Ok(ApiResponse<IEnumerable<Group>>.Ok(groups));
        }

        [HttpPost("{id}/join")]
        public async Task<IActionResult> JoinGroup(Guid id)
        {
            var userId = GetCurrentUserId();
            var result = await _groupService.JoinGroupAsync(userId, id);
            if (!result) 
                return BadRequest(ApiResponse<string>.Fail(GroupMsg.Member.JoinFail));
            
            return Ok(ApiResponse<string>.Ok(GroupMsg.Member.JoinSuccess));
        }

        [HttpPost("{id}/leave")]
        public async Task<IActionResult> LeaveGroup(Guid id)
        {
            var userId = GetCurrentUserId();
            var result = await _groupService.LeaveGroupAsync(userId, id);
            if (!result) 
                return BadRequest(ApiResponse<string>.Fail(GroupMsg.Member.LeaveFail));
            
            return Ok(ApiResponse<string>.Ok(GroupMsg.Member.LeaveSuccess));
        }

        [HttpGet("{id}/members")]
        public async Task<IActionResult> GetMembers(Guid id)
        {
            var members = await _groupService.GetGroupMembersAsync(id);
            return Ok(ApiResponse<IEnumerable<User>>.Ok(members));
        }

        [HttpPost("{id}/members/{targetId}/add")]
        public async Task<IActionResult> AddMember(Guid id, Guid targetId)
        {
            var userId = GetCurrentUserId();
            var result = await _groupService.AddMemberAsync(userId, id, targetId);
            if (!result) 
                return BadRequest(ApiResponse<string>.Fail(GroupMsg.Member.AddFail));
            
            return Ok(ApiResponse<string>.Ok(GroupMsg.Member.AddSuccess));
        }

        [HttpDelete("{id}/members/{targetId}/remove")]
        public async Task<IActionResult> RemoveMember(Guid id, Guid targetId)
        {
            var userId = GetCurrentUserId();
            var result = await _groupService.RemoveMemberAsync(userId, id, targetId);
            if (!result) 
                return BadRequest(ApiResponse<string>.Fail(GroupMsg.Member.RemoveFail));
            
            return Ok(ApiResponse<string>.Ok(GroupMsg.Member.RemoveSuccess));
        }

        [HttpGet("{id}/conversation")]
        public async Task<IActionResult> GetConversationId(Guid id)
        {
            var conversationId = await _groupService.GetConversationIdAsync(id);
            if (conversationId == null)
                return NotFound(ApiResponse<string>.Fail(GroupMsg.Get.NotFound));
            
            return Ok(ApiResponse<Guid>.Ok(conversationId.Value));
        }

        [HttpGet("suggested")]
        public async Task<IActionResult> GetSuggestedGroups()
        {
            var userId = GetCurrentUserId();
            var groups = await _groupService.GetSuggestedGroupsAsync(userId);
            return Ok(ApiResponse<IEnumerable<Group>>.Ok(groups));
        }

        [HttpPost("{id}/members/{friendId}/invite")]
        public async Task<IActionResult> InviteMember(Guid id, Guid friendId)
        {
            var userId = GetCurrentUserId();
            var result = await _groupService.InviteToGroupAsync(userId, id, friendId);
            if (!result)
                return BadRequest(ApiResponse<string>.Fail(GroupMsg.Member.InviteFail));

            return Ok(ApiResponse<string>.Ok(GroupMsg.Member.InviteSuccess));
        }

        [HttpGet("{id}/topic-users")]
        public async Task<IActionResult> GetUsersWithSameTopic(Guid id)
        {
            var users = await _groupService.GetUsersWithSameTopicAsync(id);
            return Ok(ApiResponse<IEnumerable<User>>.Ok(users));
        }

        [HttpPost("{id}/avatar")]
        public async Task<IActionResult> UploadGroupAvatar(Guid id, IFormFile file)
        {
            var userId = GetCurrentUserId();
            var isGroupAdmin = await _groupService.IsAdminAsync(userId, id);
            var isSystemAdmin = User.IsInRole("Admin");
            if (!isGroupAdmin && !isSystemAdmin)
            {
                return Unauthorized(ApiResponse<object>.Fail("Bạn không có quyền quản trị nhóm này"));
            }

            if (file == null || file.Length == 0)
                return BadRequest(ApiResponse<object>.Fail("Vui lòng chọn file ảnh"));

            var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
            if (!allowedTypes.Contains(file.ContentType.ToLower()))
                return BadRequest(ApiResponse<object>.Fail("Định dạng file không hợp lệ"));

            var avatarDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "avatars");
            Directory.CreateDirectory(avatarDir);

            var ext = Path.GetExtension(file.FileName);
            var fileName = $"{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(avatarDir, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var avatarUrl = $"avatars/{fileName}";
            var success = await _groupService.UpdateGroupAvatarAsync(id, avatarUrl);

            if (success)
                return Ok(ApiResponse<object>.Ok(new { avatarUrl }));

            return BadRequest(ApiResponse<object>.Fail("Cập nhật ảnh đại diện nhóm thất bại"));
        }
    }
}
