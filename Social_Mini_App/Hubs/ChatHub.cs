using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using MiniSocialNetwork.Data;
using Social_Mini_App.Models;
using System.Collections.Concurrent;

namespace Social_Mini_App.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly DataContext _context;
        public ChatHub(DataContext context) => _context = context;

        // Danh sách userId đang online (lưu in-memory, không cần DB)
        public static ConcurrentDictionary<string, string> OnlineUsers = new();

        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;
            if (userId != null && Guid.TryParse(userId, out var guid))
            {
                OnlineUsers[userId] = Context.ConnectionId;
                // Broadcast cho tất cả: user này vừa online
                await Clients.Others.SendAsync("UserOnline", guid);

                // Tự động join vào tất cả SignalR Groups (group chat) của user
                var groupIds = await _context.ConversationParticipants
                    .Where(cp => cp.UserId == guid)
                    .Join(_context.Conversations.Where(c => c.IsGroupChat),
                          cp => cp.ConversationId,
                          c => c.ConversationId,
                          (cp, c) => c.ConversationId.ToString())
                    .ToListAsync();

                foreach (var groupId in groupIds)
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, $"group_{groupId}");
                }
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.UserIdentifier;
            if (userId != null && Guid.TryParse(userId, out var guid))
            {
                OnlineUsers.TryRemove(userId, out _);
                // Broadcast cho tất cả: user này vừa offline
                await Clients.Others.SendAsync("UserOffline", guid);
            }
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendPrivateMessage(Guid receiverId, string content, string? imageUrl)
        {
            if (!Guid.TryParse(Context.UserIdentifier, out var senderId))
                return;

            // Validate content length and presence
            bool hasContent = !string.IsNullOrWhiteSpace(content);
            bool hasImage = !string.IsNullOrWhiteSpace(imageUrl);

            if ((!hasContent && !hasImage) || (hasContent && content.Length > 500))
            {
                await Clients.Caller.SendAsync("ReceiveError", "Tin nhắn không hợp lệ hoặc quá dài (tối đa 500 ký tự)!");
                return;
            }

            // Find or create conversation for 1-1 chat
            var conversationId = await _context.ConversationParticipants
                .Where(cp => cp.UserId == senderId)
                .Join(_context.ConversationParticipants.Where(cp2 => cp2.UserId == receiverId),
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
                var newConversation = new Conversation
                {
                    ConversationId = Guid.NewGuid(),
                    IsGroupChat = false,
                    CreatedAt = DateTime.Now
                };
                
                var participants = new List<ConversationParticipant>
                {
                    new ConversationParticipant { ParticipantId = Guid.NewGuid(), ConversationId = newConversation.ConversationId, UserId = senderId },
                    new ConversationParticipant { ParticipantId = Guid.NewGuid(), ConversationId = newConversation.ConversationId, UserId = receiverId }
                };

                await _context.Conversations.AddAsync(newConversation);
                await _context.ConversationParticipants.AddRangeAsync(participants);
                await _context.SaveChangesAsync();
                conversationId = newConversation.ConversationId;
            }

            var chatMsg = new Message
            {
                MessageId = Guid.NewGuid(),
                SenderId = senderId,
                ConversationId = conversationId,
                MessageContent = content,
                ImageUrl = imageUrl,
                CreatedAt = DateTime.Now,
                IsRead = false
            };
            
            _context.Messages.Add(chatMsg);
            await _context.SaveChangesAsync();

            await Clients.User(receiverId.ToString()).SendAsync("ReceiveMessage", senderId, content, imageUrl, chatMsg.CreatedAt);
            await Clients.Caller.SendAsync("ReceiveMessage", senderId, content, imageUrl, chatMsg.CreatedAt);
        }

        public async Task SendTypingStatus(Guid receiverId, bool isTyping)
        {
            var senderId = Context.UserIdentifier;
            if (senderId != null)
            {
                await Clients.User(receiverId.ToString()).SendAsync("UserTyping", senderId, isTyping);
            }
        }

        public async Task NotifySeen(Guid otherUserId)
        {
            var currentUserId = Context.UserIdentifier;
            if (currentUserId != null)
            {
                // Thông báo cho 'otherUserId' rằng 'currentUserId' đã xem tin nhắn
                await Clients.User(otherUserId.ToString()).SendAsync("MessageSeen", currentUserId);
            }
        }

        // Cho phép client yêu cầu join vào một SignalR group
        public async Task JoinGroup(Guid groupId)
        {
            if (!Guid.TryParse(Context.UserIdentifier, out var userId)) return;

            // Verify the user is actually a member of this group
            var isMember = await _context.ConversationParticipants
                .AnyAsync(cp => cp.ConversationId == groupId && cp.UserId == userId);

            if (isMember)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"group_{groupId}");
            }
        }

        // Thông báo cho các thành viên khác rằng có nhóm mới được tạo
        public async Task NotifyNewGroup(Guid groupId, List<Guid> memberIds, string groupName)
        {
            // Gửi cho tất cả memberIds (trừ người gửi vì người gửi đã tự add rồi)
            var currentUserId = Context.UserIdentifier;
            var otherMembers = memberIds.Where(id => id.ToString() != currentUserId).Select(id => id.ToString()).ToList();
            
            await Clients.Users(otherMembers).SendAsync("OnNewGroupCreated", new {
                ConversationId = groupId,
                Title = groupName,
                MemberCount = memberIds.Count,
                CreatedAt = DateTime.Now
            });
        }

        public async Task SendGroupMessage(Guid groupId, string content, string? imageUrl)
        {
            if (!Guid.TryParse(Context.UserIdentifier, out var senderId))
                return;

            bool hasContent = !string.IsNullOrWhiteSpace(content);
            bool hasImage = !string.IsNullOrWhiteSpace(imageUrl);

            if ((!hasContent && !hasImage) || (hasContent && content.Length > 500))
            {
                await Clients.Caller.SendAsync("ReceiveError", "Tin nhắn không hợp lệ hoặc quá dài!");
                return;
            }

            // Verify sender is a member of the group
            var isMember = await _context.ConversationParticipants
                .AnyAsync(cp => cp.ConversationId == groupId && cp.UserId == senderId);

            if (!isMember)
            {
                await Clients.Caller.SendAsync("ReceiveError", "Bạn không phải thành viên nhóm này!");
                return;
            }

            var chatMsg = new Message
            {
                MessageId = Guid.NewGuid(),
                SenderId = senderId,
                ConversationId = groupId,
                MessageContent = content,
                ImageUrl = imageUrl,
                CreatedAt = DateTime.Now,
                IsRead = false
            };

            _context.Messages.Add(chatMsg);
            await _context.SaveChangesAsync();

            // Get sender info for display
            var sender = await _context.Users.FindAsync(senderId);
            var senderName = sender?.FullName ?? sender?.Username ?? "Unknown";
            var senderAvatar = sender?.AvatarUrl;

            // Broadcast to all members of the group via SignalR group
            await Clients.Group($"group_{groupId}").SendAsync(
                "ReceiveGroupMessage",
                chatMsg.MessageId,
                groupId,
                senderId,
                senderName,
                senderAvatar,
                content,
                imageUrl,
                chatMsg.CreatedAt
            );

            // Gửi riêng cho người gọi để đảm bảo tin nhắn hiển thị ngay lập tức trên màn hình của họ
            await Clients.Caller.SendAsync(
                "ReceiveGroupMessage",
                chatMsg.MessageId,
                groupId,
                senderId,
                senderName,
                senderAvatar,
                content,
                imageUrl,
                chatMsg.CreatedAt
            );
        }
    }
}