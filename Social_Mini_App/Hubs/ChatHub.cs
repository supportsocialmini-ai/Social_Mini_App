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

        // Danh sách người đang tìm bạn (userId -> Thông tin yêu cầu)
        public class RandomQueueItem
        {
            public string TargetGender { get; set; } = "all";
            public int MinAge { get; set; }
            public int MaxAge { get; set; }
            public int UserAge { get; set; }
            public string UserGender { get; set; } = "other";
            public HashSet<string> BlockedUserIds { get; set; } = new(StringComparer.OrdinalIgnoreCase);
            public DateTime JoinedAt { get; set; } = DateTime.Now;
        }
        public static ConcurrentDictionary<string, RandomQueueItem> RandomQueue = new();

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
                if (RandomQueue.TryRemove(userId, out _))
                {
                    await BroadcastQueueUpdate();
                }
                // Broadcast cho tất cả: user này vừa offline
                await Clients.Others.SendAsync("UserOffline", guid);
            }
            await base.OnDisconnectedAsync(exception);
        }

        // --- RANDOM CHAT LOGIC ---

        private async Task BroadcastQueueUpdate()
        {
            int total = RandomQueue.Count;
            await Clients.All.SendAsync("GlobalQueueUpdate", total);

            foreach (var item in RandomQueue)
            {
                await SendStatusToUser(item.Key, item.Value);
            }
        }

        private async Task SendStatusToUser(string userId, RandomQueueItem myRequest)
        {
            int total = RandomQueue.Count;
            // Tính số người PHÙ HỢP THỰC SỰ (đã bao gồm cả Age và Blocked)
            int matchingTarget = 0;
            foreach (var p in RandomQueue)
            {
                if (IsEligibleMatch(userId, myRequest, p.Key, p.Value, myRequest.BlockedUserIds, out _))
                {
                    matchingTarget++;
                }
            }
            await Clients.User(userId).SendAsync("WaitingInQueue", total, matchingTarget);
        }

        private bool IsEligibleMatch(string myId, RandomQueueItem me, string partnerId, RandomQueueItem p, HashSet<string> myBlockedSet, out string reason)
        {
            reason = "";
            if (myId.Equals(partnerId, StringComparison.OrdinalIgnoreCase)) return false;

            // 1. Giới tính
            bool genderMatch = (p.TargetGender.Equals("all", StringComparison.OrdinalIgnoreCase) || p.TargetGender.Equals(me.UserGender, StringComparison.OrdinalIgnoreCase)) && 
                             (me.TargetGender.Equals("all", StringComparison.OrdinalIgnoreCase) || me.TargetGender.Equals(p.UserGender, StringComparison.OrdinalIgnoreCase));
            if (!genderMatch) { reason = "Lệch giới tính"; return false; }

            // 2. Độ tuổi
            bool ageMatch = (me.UserAge >= p.MinAge && me.UserAge <= p.MaxAge) && (p.UserAge >= me.MinAge && p.UserAge <= me.MaxAge);
            if (!ageMatch) { reason = "Lệch tuổi"; return false; }

            // 3. Người quen
            if (myBlockedSet.Contains(partnerId) || p.BlockedUserIds.Contains(myId)) { reason = "Đã là bạn bè/người quen"; return false; }

            return true;
        }

        public async Task JoinRandomQueue(string targetGender, int minAge = 18, int maxAge = 99)
        {
            try
            {
                var userId = Context.UserIdentifier;
                if (userId == null || !Guid.TryParse(userId, out var userIdGuid)) return;

                var currentUser = await _context.Users.FindAsync(userIdGuid);
                if (currentUser == null || !currentUser.DateOfBirth.HasValue)
                {
                    await Clients.Caller.SendAsync("ReceiveError", "Vui lòng cập nhật đầy đủ Ngày sinh và Giới tính trong Hồ sơ!");
                    return;
                }

                int myAge = CalculateAge(currentUser.DateOfBirth.Value);
                var myGender = GetNormalizedGender(currentUser.Gender);
                var normTargetGender = GetNormalizedGender(targetGender);

                // 1. LẤY DANH SÁCH NGƯỜI QUEN (ĐƠN GIẢN HÓA)
                // Lấy ID những người đã là bạn bè
                var friendIds = await _context.FriendshipMembers
                    .Where(fm => fm.UserId == userIdGuid)
                    .Select(fm => fm.FriendshipId)
                    .SelectMany(fid => _context.FriendshipMembers
                        .Where(fm2 => fm2.FriendshipId == fid && fm2.UserId != userIdGuid)
                        .Select(fm2 => fm2.UserId.ToString()))
                    .ToListAsync();

                // Lấy ID những người đã từng chat công khai
                var chatPartnerIds = await _context.ConversationParticipants
                    .Where(cp => cp.UserId == userIdGuid)
                    .Select(cp => cp.ConversationId)
                    .SelectMany(cid => _context.ConversationParticipants
                        .Where(cp2 => cp2.ConversationId == cid && cp2.UserId != userIdGuid)
                        .Join(_context.Conversations.Where(c => !c.IsAnonymous), 
                              cp2 => cp2.ConversationId, c => c.ConversationId, 
                              (cp2, c) => cp2.UserId.ToString()))
                    .ToListAsync();

                var blockedSet = new HashSet<string>(friendIds.Concat(chatPartnerIds), StringComparer.OrdinalIgnoreCase);

                // LOG CHẨN ĐOÁN
                Console.WriteLine($"[Diagnosis] User join: {currentUser.FullName} ({userId})");
                Console.WriteLine($" >> Age: {myAge}, Gender: {myGender}, Target: {normTargetGender} ({minAge}-{maxAge})");
                Console.WriteLine($" >> Blocked Count: {blockedSet.Count}");

                // Tạo object request cho chính mình
                var myRequest = new RandomQueueItem { 
                    TargetGender = normTargetGender, 
                    MinAge = minAge, MaxAge = maxAge, 
                    UserAge = myAge, UserGender = myGender,
                    BlockedUserIds = blockedSet,
                    JoinedAt = DateTime.Now
                };

                // Tìm đối tác - FIFO
                KeyValuePair<string, RandomQueueItem> partnerEntry = default;
                foreach (var x in RandomQueue.OrderBy(q => q.Value.JoinedAt))
                {
                    if (IsEligibleMatch(userId, myRequest, x.Key, x.Value, blockedSet, out var reason))
                    {
                        partnerEntry = x;
                        break;
                    }
                    else if (!x.Key.Equals(userId, StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine($" >> Match Skip with {x.Key}: {reason}");
                    }
                }

                if (!string.IsNullOrEmpty(partnerEntry.Key))
                {
                    var partnerId = partnerEntry.Key;
                    if (RandomQueue.TryRemove(partnerId, out _))
                    {
                        var partnerGuid = Guid.Parse(partnerId);
                        var conversation = new Conversation { ConversationId = Guid.NewGuid(), IsAnonymous = true, CreatedAt = DateTime.Now };
                        var participants = new List<ConversationParticipant> {
                            new ConversationParticipant { ParticipantId = Guid.NewGuid(), ConversationId = conversation.ConversationId, UserId = userIdGuid, JoinedAt = DateTime.Now },
                            new ConversationParticipant { ParticipantId = Guid.NewGuid(), ConversationId = conversation.ConversationId, UserId = partnerGuid, JoinedAt = DateTime.Now }
                        };
                        await _context.Conversations.AddAsync(conversation);
                        await _context.ConversationParticipants.AddRangeAsync(participants);
                        await _context.SaveChangesAsync();

                        await Clients.User(userId).SendAsync("RandomMatchFound", conversation.ConversationId, $"Người lạ, {partnerEntry.Value.UserAge}", partnerId);
                        await Clients.User(partnerId).SendAsync("RandomMatchFound", conversation.ConversationId, $"Người lạ, {myAge}", userId);
                        await BroadcastQueueUpdate();
                        return;
                    }
                }

                // Nếu chưa khớp, thêm vào hàng chờ
                RandomQueue[userId] = myRequest;
                await BroadcastQueueUpdate();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] {ex.Message}");
                await Clients.Caller.SendAsync("ReceiveError", "Lỗi matching!");
            }
        }

        public async Task UpdateQueueStatus()
        {
            var userId = Context.UserIdentifier;
            if (userId != null && RandomQueue.TryGetValue(userId, out var req))
            {
                await SendStatusToUser(userId, req);
            }
        }

        private string GetNormalizedGender(string? gender)
        {
            if (string.IsNullOrWhiteSpace(gender)) return "other";
            
            var g = gender.Trim().ToLower();
            if (g == "male" || g == "nam" || g == "boy") return "male";
            if (g == "female" || g == "nữ" || g == "nu" || g == "girl") return "female";
            
            return "other";
        }

        private int CalculateAge(DateTime dob)
        {
            var today = DateTime.Today;
            var age = today.Year - dob.Year;
            if (dob.Date > today.AddYears(-age)) age--;
            return age;
        }

        public async Task LeaveRandomQueue()
        {
            var userId = Context.UserIdentifier;
            if (userId != null)
            {
                RandomQueue.TryRemove(userId, out _);
            }
        }

        public async Task SendRandomHeart(Guid conversationId)
        {
            if (!Guid.TryParse(Context.UserIdentifier, out var userId)) return;

            var conversation = await _context.Conversations
                .Include(c => c.Participants)
                .FirstOrDefaultAsync(c => c.ConversationId == conversationId);

            if (conversation == null || !conversation.IsAnonymous) return;

            // Xác định xem mình là User1 hay User2 trong bảng này (tạm thời dựa vào thứ tự Participant)
            var participants = conversation.Participants.OrderBy(p => p.ParticipantId).ToList();
            if (participants.Count < 2) return;

            if (participants[0].UserId == userId) conversation.User1Matched = true;
            else if (participants[1].UserId == userId) conversation.User2Matched = true;

            await _context.SaveChangesAsync();

            // Thông báo cho đối phương
            var partner = participants.FirstOrDefault(p => p.UserId != userId);
            if (partner != null)
            {
                await Clients.User(partner.UserId.ToString()).SendAsync("PartnerLiked");
            }

            // Kiểm tra nếu cả hai cùng bấm Tim
            if (conversation.User1Matched && conversation.User2Matched)
            {
                conversation.IsAnonymous = false; // "Chính thức hóa" cuộc trò chuyện
                await _context.SaveChangesAsync();

                // Reveal identity data
                var user1 = await _context.Users.FindAsync(participants[0].UserId);
                var user2 = await _context.Users.FindAsync(participants[1].UserId);

                // Lộ diện danh tính cho từng người (Dùng ID chữ thường cho chuẩn)
                await Clients.User(participants[0].UserId.ToString().ToLower()).SendAsync("IdentityRevealed", new { 
                    fullName = user2?.FullName, 
                    avatarUrl = user2?.AvatarUrl,
                    userId = user2?.UserId.ToString().ToLower()
                });
                await Clients.User(participants[1].UserId.ToString().ToLower()).SendAsync("IdentityRevealed", new { 
                    fullName = user1?.FullName, 
                    avatarUrl = user1?.AvatarUrl,
                    userId = user1?.UserId.ToString().ToLower()
                });

                // Gửi tín hiệu để cả 2 bên cùng tự động chuyển trang
                await Clients.User(participants[0].UserId.ToString().ToLower()).SendAsync("ChatMatured", conversationId);
                await Clients.User(participants[1].UserId.ToString().ToLower()).SendAsync("ChatMatured", conversationId);
            }
        }

        public async Task LeaveRandomChat(Guid conversationId)
        {
            if (!Guid.TryParse(Context.UserIdentifier, out var userId)) return;

            var conversation = await _context.Conversations.FindAsync(conversationId);
            if (conversation == null) return;

            if (conversation.IsAnonymous)
            {
                // Nếu chưa thành cuộc trò chuyện chính, xóa sạch dữ liệu
                var messages = _context.Messages.Where(m => m.ConversationId == conversationId);
                _context.Messages.RemoveRange(messages);
                _context.Conversations.Remove(conversation);
                await _context.SaveChangesAsync();
            }

            // Thông báo cho bên kia
            var participants = await _context.ConversationParticipants
                .Where(cp => cp.ConversationId == conversationId && cp.UserId != userId)
                .ToListAsync();
            
            foreach (var p in participants)
            {
                await Clients.User(p.UserId.ToString()).SendAsync("PartnerLeft");
            }
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