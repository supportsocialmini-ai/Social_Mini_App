using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniSocialNetwork.Data;
using MiniSocialNetwork.Wrappers;
using Social_Mini_App.Dtos.Requests;
using Social_Mini_App.Hubs;
using Social_Mini_App.Models;
using Social_Mini_App.Messages;
using System.Security.Claims;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly DataContext _context;
    public ChatController(DataContext context) => _context = context;

    [HttpGet("{otherUserId}")]
    public async Task<IActionResult> GetChatHistory(Guid otherUserId)
    {
        var currentUserIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserIdStr == null || !Guid.TryParse(currentUserIdStr, out var currentUserId)) 
            return Unauthorized(ApiResponse<List<Message>>.Fail(UserMsg.Profile.Unauthorized));

        // Find or create conversation for 1-1 chat
        var conversationId = await _context.ConversationParticipants
            .Where(cp => cp.UserId == currentUserId)
            .Join(_context.ConversationParticipants.Where(cp2 => cp2.UserId == otherUserId),
                  cp => cp.ConversationId,
                  cp2 => cp2.ConversationId,
                  (cp, cp2) => cp.ConversationId)
            .Join(_context.Conversations.Where(c => !c.IsGroupChat),
                  cid => cid,
                  c => c.ConversationId,
                  (cid, c) => cid)
            .FirstOrDefaultAsync();

        if (conversationId == Guid.Empty)
        {
            return Ok(ApiResponse<List<Message>>.Ok(new List<Message>()));
        }

        var messages = await _context.Messages
            .Where(m => m.ConversationId == conversationId)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync();

        return Ok(ApiResponse<List<Message>>.Ok(messages));
    }

    [HttpGet("users-to-chat")]
    public async Task<IActionResult> GetUsersToChat()
    {
        var currentUserIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserIdStr == null || !Guid.TryParse(currentUserIdStr, out var currentUserId)) 
            return Unauthorized(ApiResponse<object>.Fail(UserMsg.Profile.Unauthorized));

        var users = await _context.ConversationParticipants
            .Where(cp => cp.UserId == currentUserId)
            // Join to find the *other* participants in the same conversation
            .SelectMany(cp => _context.ConversationParticipants
                .Where(cp2 => cp2.ConversationId == cp.ConversationId && cp2.UserId != currentUserId))
            // Get the last message time for each conversation to check if it has messages
            .Select(cp2 => new
            {
                cp2.UserId,
                cp2.ConversationId,
                LastMessageTime = _context.Messages
                    .Where(m => m.ConversationId == cp2.ConversationId)
                    .Max(m => (DateTime?)m.CreatedAt)
            })
            // Only include conversations that actually have at least one message
            .Where(x => x.LastMessageTime != null)
            // Sort by most recently active conversation
            .OrderByDescending(x => x.LastMessageTime)
            // Join with Users table to get the display info
            .Join(_context.Users, x => x.UserId, u => u.UserId, (x, u) => new
            {
                u.UserId,
                u.Username,
                FullName = u.FullName,
                u.AvatarUrl
            })
            // Distinct in case there are multiple shared conversations (e.g. groups if enabled later)
            .Distinct()
            .ToListAsync();

        return Ok(ApiResponse<object>.Ok(users));
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        var currentUserIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserIdStr == null || !Guid.TryParse(currentUserIdStr, out var currentUserId)) 
            return Unauthorized(ApiResponse<int>.Fail(UserMsg.Profile.Unauthorized));

        var count = await _context.ConversationParticipants
            .Where(cp => cp.UserId == currentUserId)
            .Join(_context.Messages.Where(m => m.SenderId != currentUserId && !m.IsRead),
                  cp => cp.ConversationId,
                  m => m.ConversationId,
                  (cp, m) => m)
            .CountAsync();

        return Ok(ApiResponse<int>.Ok(count));
    }

    [HttpGet("unread-counts")]
    public async Task<IActionResult> GetUnreadCountsPerUser()
    {
        var currentUserIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserIdStr == null || !Guid.TryParse(currentUserIdStr, out var currentUserId)) 
            return Unauthorized(ApiResponse<object>.Fail(UserMsg.Profile.Unauthorized));

        var counts = await _context.ConversationParticipants
            .Where(cp => cp.UserId == currentUserId)
            .Join(_context.Messages.Where(m => m.SenderId != currentUserId && !m.IsRead),
                  cp => cp.ConversationId,
                  m => m.ConversationId,
                  (cp, m) => m)
            .GroupBy(m => m.SenderId)
            .Select(g => new { SenderId = g.Key, Count = g.Count() })
            .ToListAsync();

        return Ok(ApiResponse<object>.Ok(counts));
    }

    [HttpPost("{otherUserId}/read")]
    public async Task<IActionResult> MarkAsRead(Guid otherUserId)
    {
        var currentUserIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserIdStr == null || !Guid.TryParse(currentUserIdStr, out var currentUserId)) 
            return Unauthorized(ApiResponse<string>.Fail(UserMsg.Profile.Unauthorized));

        var conversationId = await _context.ConversationParticipants
            .Where(cp => cp.UserId == currentUserId)
            .Join(_context.ConversationParticipants.Where(cp2 => cp2.UserId == otherUserId),
                  cp => cp.ConversationId,
                  cp2 => cp2.ConversationId,
                  (cp, cp2) => cp.ConversationId)
            .FirstOrDefaultAsync();

        if (conversationId != Guid.Empty)
        {
            var unreadMessages = await _context.Messages
                .Where(m => m.ConversationId == conversationId && m.SenderId != currentUserId && !m.IsRead)
                .ToListAsync();

            if (unreadMessages.Any())
            {
                foreach (var m in unreadMessages)
                {
                    m.IsRead = true;
                }
                await _context.SaveChangesAsync();
            }
        }

        return Ok(ApiResponse<string>.Ok(ChatMsg.Action.ReadSuccess));
    }

    [HttpGet("online-users")]
    public IActionResult GetOnlineUsers()
    {
        var onlineUserIds = new List<Guid>();
        foreach (var idStr in ChatHub.OnlineUsers.Keys)
        {
            if (Guid.TryParse(idStr, out var guid))
                onlineUserIds.Add(guid);
        }
        return Ok(ApiResponse<List<Guid>>.Ok(onlineUserIds));
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadImage(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(ApiResponse<string>.Fail(UserMsg.Avatar.FileRequired));

        try
        {
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "chat");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var fileName = $"{Guid.NewGuid()}_{file.FileName}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var imageUrl = $"images/chat/{fileName}";
            return Ok(ApiResponse<object>.Ok(new { imageUrl }));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<string>.Fail(ChatMsg.Upload.Fail, ex.Message));
        }
    }

    // ==================== GROUP CHAT ====================

    [HttpPost("group")]
    public async Task<IActionResult> CreateGroup([FromBody] CreateGroupRequest request)
    {
        var currentUserIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserIdStr == null || !Guid.TryParse(currentUserIdStr, out var currentUserId))
            return Unauthorized(ApiResponse<object>.Fail(UserMsg.Profile.Unauthorized));

        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(ApiResponse<string>.Fail("Tên nhóm không được để trống!"));

        if (request.MemberIds == null || request.MemberIds.Count < 1)
            return BadRequest(ApiResponse<string>.Fail("Nhóm phải có ít nhất 2 thành viên!"));

        var group = new Conversation
        {
            ConversationId = Guid.NewGuid(),
            Title = request.Name.Trim(),
            IsGroupChat = true,
            CreatorId = currentUserId,
            CreatedAt = DateTime.Now
        };

        // Add creator as admin participant
        var participants = new List<ConversationParticipant>
        {
            new ConversationParticipant
            {
                ParticipantId = Guid.NewGuid(),
                ConversationId = group.ConversationId,
                UserId = currentUserId,
                IsAdmin = true,
                JoinedAt = DateTime.Now
            }
        };

        // Add other members
        foreach (var memberId in request.MemberIds.Distinct())
        {
            if (memberId == currentUserId) continue;
            participants.Add(new ConversationParticipant
            {
                ParticipantId = Guid.NewGuid(),
                ConversationId = group.ConversationId,
                UserId = memberId,
                IsAdmin = false,
                JoinedAt = DateTime.Now
            });
        }

        await _context.Conversations.AddAsync(group);
        await _context.ConversationParticipants.AddRangeAsync(participants);
        await _context.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(new { group.ConversationId, group.Title, group.CreatedAt }));
    }

    [HttpGet("groups")]
    public async Task<IActionResult> GetMyGroups()
    {
        var currentUserIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserIdStr == null || !Guid.TryParse(currentUserIdStr, out var currentUserId))
            return Unauthorized(ApiResponse<object>.Fail(UserMsg.Profile.Unauthorized));

        var groups = await _context.ConversationParticipants
            .Where(cp => cp.UserId == currentUserId)
            .Join(_context.Conversations.Where(c => c.IsGroupChat),
                  cp => cp.ConversationId,
                  c => c.ConversationId,
                  (cp, c) => new
                  {
                      c.ConversationId,
                      c.Title,
                      c.AvatarUrl,
                      c.CreatorId,
                      c.CreatedAt,
                      cp.IsAdmin,
                      LastMessageTime = _context.Messages
                          .Where(m => m.ConversationId == c.ConversationId)
                          .Max(m => (DateTime?)m.CreatedAt),
                      LastMessage = _context.Messages
                          .Where(m => m.ConversationId == c.ConversationId)
                          .OrderByDescending(m => m.CreatedAt)
                          .Select(m => m.MessageContent)
                          .FirstOrDefault(),
                      MemberCount = _context.ConversationParticipants
                          .Count(p => p.ConversationId == c.ConversationId),
                      UnreadCount = _context.Messages
                          .Count(m => m.ConversationId == c.ConversationId && m.SenderId != currentUserId && !m.IsRead)
                  })
            .OrderByDescending(g => g.LastMessageTime)
            .ToListAsync();

        return Ok(ApiResponse<object>.Ok(groups));
    }

    [HttpGet("group/{groupId}")]
    public async Task<IActionResult> GetGroupMessages(Guid groupId)
    {
        var currentUserIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserIdStr == null || !Guid.TryParse(currentUserIdStr, out var currentUserId))
            return Unauthorized(ApiResponse<object>.Fail(UserMsg.Profile.Unauthorized));

        var isMember = await _context.ConversationParticipants
            .AnyAsync(cp => cp.ConversationId == groupId && cp.UserId == currentUserId);

        if (!isMember)
            return Forbid();

        var messages = await _context.Messages
            .Where(m => m.ConversationId == groupId)
            .OrderBy(m => m.CreatedAt)
            .Join(_context.Users, m => m.SenderId, u => u.UserId, (m, u) => new
            {
                m.MessageId,
                m.SenderId,
                SenderName = u.FullName ?? u.Username,
                SenderAvatar = u.AvatarUrl,
                m.MessageContent,
                m.ImageUrl,
                m.CreatedAt,
                m.IsRead
            })
            .ToListAsync();

        return Ok(ApiResponse<object>.Ok(messages));
    }

    [HttpPost("group/{groupId}/read")]
    public async Task<IActionResult> MarkGroupAsRead(Guid groupId)
    {
        var currentUserIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserIdStr == null || !Guid.TryParse(currentUserIdStr, out var currentUserId))
            return Unauthorized(ApiResponse<string>.Fail(UserMsg.Profile.Unauthorized));

        var unread = await _context.Messages
            .Where(m => m.ConversationId == groupId && m.SenderId != currentUserId && !m.IsRead)
            .ToListAsync();

        foreach (var m in unread) m.IsRead = true;
        await _context.SaveChangesAsync();

        return Ok(ApiResponse<string>.Ok("Marked as read"));
    }

    [HttpGet("group/{groupId}/members")]
    public async Task<IActionResult> GetGroupMembers(Guid groupId)
    {
        var currentUserIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserIdStr == null || !Guid.TryParse(currentUserIdStr, out var currentUserId))
            return Unauthorized(ApiResponse<object>.Fail(UserMsg.Profile.Unauthorized));

        var isMember = await _context.ConversationParticipants
            .AnyAsync(cp => cp.ConversationId == groupId && cp.UserId == currentUserId);
        if (!isMember) return Forbid();

        var members = await _context.ConversationParticipants
            .Where(cp => cp.ConversationId == groupId)
            .Join(_context.Users, cp => cp.UserId, u => u.UserId, (cp, u) => new
            {
                u.UserId,
                u.Username,
                FullName = u.FullName,
                u.AvatarUrl,
                cp.IsAdmin,
                cp.JoinedAt
            })
            .ToListAsync();

        return Ok(ApiResponse<object>.Ok(members));
    }
}