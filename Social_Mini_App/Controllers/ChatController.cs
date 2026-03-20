using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniSocialNetwork.Data;
using MiniSocialNetwork.Wrappers;
using Social_Mini_App.Hubs;
using Social_Mini_App.Models;
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
            return Unauthorized(ApiResponse<List<Message>>.Fail("Unauthorized"));

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
            return Unauthorized(ApiResponse<object>.Fail("Unauthorized"));

        var users = await _context.Users
            .Where(u => u.UserId != currentUserId)
            .Select(u => new
            {
                u.UserId,
                u.Username,
                FullName = u.FullName,
                u.AvatarUrl
            })
            .ToListAsync();

        return Ok(ApiResponse<object>.Ok(users));
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        var currentUserIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserIdStr == null || !Guid.TryParse(currentUserIdStr, out var currentUserId)) 
            return Unauthorized(ApiResponse<int>.Fail("Unauthorized"));

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
            return Unauthorized(ApiResponse<object>.Fail("Unauthorized"));

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
            return Unauthorized(ApiResponse<string>.Fail("Unauthorized"));

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

        return Ok(ApiResponse<string>.Ok("Success"));
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
            return BadRequest(ApiResponse<string>.Fail("File is empty"));

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
            return BadRequest(ApiResponse<string>.Fail($"Error uploading image: {ex.Message}"));
        }
    }
}