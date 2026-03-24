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

            // Validate content length
            if (string.IsNullOrWhiteSpace(content) || content.Length > 500)
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
    }
}