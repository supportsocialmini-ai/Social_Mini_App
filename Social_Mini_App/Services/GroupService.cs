using Microsoft.EntityFrameworkCore;
using MiniSocialNetwork.Data;
using MiniSocialNetwork.Interfaces;
using MiniSocialNetwork.Models;
using Social_Mini_App.Dtos.Requests;
using Social_Mini_App.Interfaces;
using Social_Mini_App.Models;

namespace Social_Mini_App.Services
{
    public class GroupService : IGroupService
    {
        private readonly DataContext _context;
        private readonly INotificationService _notifService;

        public GroupService(DataContext context, INotificationService notifService)
        {
            _context = context;
            _notifService = notifService;
        }

        public async Task<Group> CreateGroupAsync(Guid creatorId, CreateGroupRequest request)
        {
            var group = new Group
            {
                GroupId = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                Privacy = request.Privacy,
                Category = request.Category,
                CreatedBy = creatorId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Groups.Add(group);

            // Thêm người tạo làm Owner
            var owner = new GroupMember
            {
                GroupId = group.GroupId,
                UserId = creatorId,
                Role = "Owner",
                JoinedAt = DateTime.UtcNow,
                Status = "Active"
            };
            _context.GroupMembers.Add(owner);

            // Thêm các thành viên được mời (nếu có)
            if (request.MemberIds != null && request.MemberIds.Any())
            {
                foreach (var memberId in request.MemberIds.Where(id => id != creatorId))
                {
                    _context.GroupMembers.Add(new GroupMember
                    {
                        GroupId = group.GroupId,
                        UserId = memberId,
                        Role = "Member",
                        JoinedAt = DateTime.UtcNow,
                        Status = "Active" // Ở bản đơn giản này cho Active luôn, sau này có thể để Pending
                    });
                }
            }

            await _context.SaveChangesAsync();

            // Tự động tạo cuộc hội thoại cho nhóm
            var conversation = new Conversation
            {
                ConversationId = Guid.NewGuid(),
                GroupId = group.GroupId,
                IsGroupChat = true,
                Title = group.Name,
                AvatarUrl = group.AvatarUrl,
                CreatedAt = DateTime.Now,
                CreatorId = creatorId
            };
            _context.Conversations.Add(conversation);
            
            // Add owner to conversation participants
            _context.ConversationParticipants.Add(new ConversationParticipant
            {
                ConversationId = conversation.ConversationId,
                UserId = creatorId,
                JoinedAt = DateTime.Now
            });

            await _context.SaveChangesAsync();
            return group;
        }

        public async Task<Group?> GetGroupByIdAsync(Guid groupId)
        {
            return await _context.Groups
                .Include(g => g.Creator)
                .Include(g => g.Members)
                .FirstOrDefaultAsync(g => g.GroupId == groupId);
        }

        public async Task<IEnumerable<Group>> GetUserGroupsAsync(Guid userId)
        {
            return await _context.GroupMembers
                .Where(gm => gm.UserId == userId && gm.Status == "Active")
                .Include(gm => gm.Group)
                    .ThenInclude(g => g!.Conversation)
                .Select(gm => gm.Group!)
                .ToListAsync();
        }

        public async Task<IEnumerable<Group>> SearchGroupsAsync(string searchTerm)
        {
            var query = _context.Groups.Where(g => g.Privacy == "Public");

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(g => g.Name.Contains(searchTerm) || (g.Description != null && g.Description.Contains(searchTerm)));
            }

            return await query.ToListAsync();
        }

        public async Task<bool> JoinGroupAsync(Guid userId, Guid groupId)
        {
            var group = await _context.Groups.FindAsync(groupId);
            if (group == null) return false;

            var existingMember = await _context.GroupMembers
                .FirstOrDefaultAsync(gm => gm.GroupId == groupId && gm.UserId == userId);
            
            if (existingMember != null) return false;

            var member = new GroupMember
            {
                GroupId = groupId,
                UserId = userId,
                Role = "Member",
                JoinedAt = DateTime.UtcNow,
                Status = group.Privacy == "Public" ? "Active" : "Pending"
            };

            _context.GroupMembers.Add(member);
            
            // Nếu là nhóm công khai, thêm luôn vào chat
            if (group.Privacy == "Public")
            {
                var conversation = await _context.Conversations.FirstOrDefaultAsync(c => c.GroupId == groupId);
                if (conversation != null)
                {
                    _context.ConversationParticipants.Add(new ConversationParticipant
                    {
                        ConversationId = conversation.ConversationId,
                        UserId = userId,
                        JoinedAt = DateTime.Now
                    });
                }
            }

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> LeaveGroupAsync(Guid userId, Guid groupId)
        {
            var member = await _context.GroupMembers
                .FirstOrDefaultAsync(gm => gm.GroupId == groupId && gm.UserId == userId);
            
            if (member == null || member.Role == "Owner") return false;

            _context.GroupMembers.Remove(member);

            // Xóa khỏi chat
            var conversation = await _context.Conversations.FirstOrDefaultAsync(c => c.GroupId == groupId);
            if (conversation != null)
            {
                var participant = await _context.ConversationParticipants
                    .FirstOrDefaultAsync(cp => cp.ConversationId == conversation.ConversationId && cp.UserId == userId);
                if (participant != null)
                {
                    _context.ConversationParticipants.Remove(participant);
                }
            }

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> AddMemberAsync(Guid adminId, Guid groupId, Guid targetUserId)
        {
            if (!await IsAdminAsync(adminId, groupId)) return false;

            var existingMember = await _context.GroupMembers
                .FirstOrDefaultAsync(gm => gm.GroupId == groupId && gm.UserId == targetUserId);

            if (existingMember != null)
            {
                if (existingMember.Status == "Pending")
                {
                    existingMember.Status = "Active";
                    return await _context.SaveChangesAsync() > 0;
                }
                return false;
            }

            _context.GroupMembers.Add(new GroupMember
            {
                GroupId = groupId,
                UserId = targetUserId,
                Role = "Member",
                JoinedAt = DateTime.UtcNow,
                Status = "Active"
            });

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> RemoveMemberAsync(Guid adminId, Guid groupId, Guid targetUserId)
        {
            var isSystemAdmin = await _context.UserRoles
                .AnyAsync(ur => ur.UserId == adminId && ur.Role!.Name == "Admin");

            if (!await IsAdminAsync(adminId, groupId) && !isSystemAdmin) return false;

            var member = await _context.GroupMembers
                .FirstOrDefaultAsync(gm => gm.GroupId == groupId && gm.UserId == targetUserId);

            if (member == null || member.Role == "Owner") return false;

            _context.GroupMembers.Remove(member);

            // Xóa cả trong cuộc hội thoại của nhóm nếu có
            var conversation = await _context.Conversations.FirstOrDefaultAsync(c => c.GroupId == groupId);
            if (conversation != null)
            {
                var participant = await _context.ConversationParticipants
                    .FirstOrDefaultAsync(cp => cp.ConversationId == conversation.ConversationId && cp.UserId == targetUserId);
                if (participant != null)
                {
                    _context.ConversationParticipants.Remove(participant);
                }
            }

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<IEnumerable<User>> GetGroupMembersAsync(Guid groupId)
        {
            return await _context.GroupMembers
                .Where(gm => gm.GroupId == groupId && gm.Status == "Active")
                .Include(gm => gm.User)
                .Select(gm => gm.User!)
                .ToListAsync();
        }

        public async Task<bool> IsMemberAsync(Guid userId, Guid groupId)
        {
            return await _context.GroupMembers
                .AnyAsync(gm => gm.GroupId == groupId && gm.UserId == userId && gm.Status == "Active");
        }

        public async Task<bool> IsAdminAsync(Guid userId, Guid groupId)
        {
            return await _context.GroupMembers
                .AnyAsync(gm => gm.GroupId == groupId && gm.UserId == userId && 
                               (gm.Role == "Admin" || gm.Role == "Owner") && 
                               gm.Status == "Active");
        }

        public async Task<Guid?> GetConversationIdAsync(Guid groupId)
        {
            var conversation = await _context.Conversations.FirstOrDefaultAsync(c => c.GroupId == groupId);
            if (conversation == null)
            {
                var group = await _context.Groups.Include(g => g.Members).FirstOrDefaultAsync(g => g.GroupId == groupId);
                if (group == null) return null;

                // Create conversation if it doesn't exist
                conversation = new Conversation
                {
                    ConversationId = Guid.NewGuid(),
                    GroupId = groupId,
                    IsGroupChat = true,
                    CreatedAt = DateTime.Now
                };

                _context.Conversations.Add(conversation);

                // Add existing members to the conversation
                foreach (var member in group.Members.Where(m => m.Status == "Active"))
                {
                    _context.ConversationParticipants.Add(new ConversationParticipant
                    {
                        ConversationId = conversation.ConversationId,
                        UserId = member.UserId,
                        JoinedAt = DateTime.Now
                    });
                }

                await _context.SaveChangesAsync();
            }
            return conversation.ConversationId;
        }

        public async Task<IEnumerable<Group>> GetSuggestedGroupsAsync(Guid userId)
        {
            var user = await _context.Users.FindAsync(userId);
            
            // Lấy các group user CHƯA tham gia
            var myGroupIds = await _context.GroupMembers
                .Where(gm => gm.UserId == userId)
                .Select(gm => gm.GroupId)
                .ToListAsync();

            var publicGroups = await _context.Groups
                .Where(g => g.Privacy == "Public" && !myGroupIds.Contains(g.GroupId))
                .Include(g => g.Members)
                .ToListAsync();

            if (user == null)
            {
                return publicGroups.OrderByDescending(g => g.Members.Count).Take(5).ToList();
            }

            var userCategory = (user.Category ?? string.Empty).Trim().ToLower();
            var interests = (user.Interests ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(i => i.Trim().ToLower())
                .ToList();

            // Lọc các group có category khớp category của user hoặc trùng sở thích
            var matchedGroups = publicGroups
                .Where(g => !string.IsNullOrEmpty(g.Category) && 
                           ((!string.IsNullOrEmpty(userCategory) && g.Category.Trim().ToLower() == userCategory) ||
                            interests.Any(interest => g.Category.ToLower().Contains(interest))))
                .OrderByDescending(g => g.Members.Count)
                .Take(5)
                .ToList();

            // Fallback: Nếu không có group nào khớp, lấy các group phổ biến
            if (matchedGroups.Count == 0)
            {
                return publicGroups.OrderByDescending(g => g.Members.Count).Take(5).ToList();
            }

            return matchedGroups;
        }

        public async Task<bool> InviteToGroupAsync(Guid inviterId, Guid groupId, Guid friendId)
        {
            // Inviter must be an active member of the group
            var inviterMember = await _context.GroupMembers
                .FirstOrDefaultAsync(gm => gm.GroupId == groupId && gm.UserId == inviterId && gm.Status == "Active");
            if (inviterMember == null) return false;

            // Cannot invite yourself
            if (inviterId == friendId) return false;

            // Check if friend is already a member (any status)
            var existingMember = await _context.GroupMembers
                .FirstOrDefaultAsync(gm => gm.GroupId == groupId && gm.UserId == friendId);
            if (existingMember != null) return false;

            var group = await _context.Groups.FindAsync(groupId);
            if (group == null) return false;

            // Add friend as active member directly
            _context.GroupMembers.Add(new GroupMember
            {
                GroupId = groupId,
                UserId = friendId,
                Role = "Member",
                JoinedAt = DateTime.UtcNow,
                Status = "Active"
            });

            // Also add to group conversation
            var conversation = await _context.Conversations.FirstOrDefaultAsync(c => c.GroupId == groupId);
            if (conversation != null)
            {
                var alreadyParticipant = await _context.ConversationParticipants
                    .AnyAsync(cp => cp.ConversationId == conversation.ConversationId && cp.UserId == friendId);
                if (!alreadyParticipant)
                {
                    _context.ConversationParticipants.Add(new ConversationParticipant
                    {
                        ConversationId = conversation.ConversationId,
                        UserId = friendId,
                        JoinedAt = DateTime.Now
                    });
                }
            }

            var saved = await _context.SaveChangesAsync() > 0;

            if (saved)
            {
                // Notify the invited friend
                await _notifService.CreateNotifAsync(inviterId, friendId, null, "GroupInvite");
            }

            return saved;
        }

        public async Task<IEnumerable<User>> GetUsersWithSameTopicAsync(Guid groupId)
        {
            var group = await _context.Groups.FindAsync(groupId);
            if (group == null)
                return Enumerable.Empty<User>();

            // Lấy tất cả UserId của thành viên đang hoạt động trong nhóm này để loại trừ
            var currentMemberIds = await _context.GroupMembers
                .Where(gm => gm.GroupId == groupId && gm.Status == "Active")
                .Select(gm => gm.UserId)
                .ToListAsync();

            var groupCategory = (group.Category ?? string.Empty).Trim().ToLower();

            // Nếu nhóm không có chủ đề cụ thể (Category bị null hoặc trống)
            if (string.IsNullOrEmpty(groupCategory))
            {
                return Enumerable.Empty<User>();
            }

            // Lấy toàn bộ người dùng hoạt động bình thường, chưa tham gia nhóm và có cùng Category với nhóm
            var matchedUsers = await _context.Users
                .Where(u => u.IsActive && !u.IsDeleted 
                           && !currentMemberIds.Contains(u.UserId) 
                           && u.Category != null 
                           && u.Category.Trim().ToLower() == groupCategory)
                .ToListAsync();

            return matchedUsers;
        }

        public async Task<bool> UpdateGroupAvatarAsync(Guid groupId, string avatarUrl)
        {
            var group = await _context.Groups.FindAsync(groupId);
            if (group == null) return false;

            group.AvatarUrl = avatarUrl;

            // Đồng bộ ảnh đại diện của cuộc hội thoại chat nhóm tương ứng
            var conversation = await _context.Conversations.FirstOrDefaultAsync(c => c.GroupId == groupId);
            if (conversation != null)
            {
                conversation.AvatarUrl = avatarUrl;
            }

            return await _context.SaveChangesAsync() > 0;
        }
    }
}
