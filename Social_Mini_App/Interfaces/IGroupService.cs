using MiniSocialNetwork.Models;
using Social_Mini_App.Dtos.Requests;
using Social_Mini_App.Models;

namespace MiniSocialNetwork.Interfaces
{
    public interface IGroupService
    {
        Task<Group> CreateGroupAsync(Guid creatorId, CreateGroupRequest request);
        Task<Group?> GetGroupByIdAsync(Guid groupId);
        Task<IEnumerable<Group>> GetUserGroupsAsync(Guid userId);
        Task<IEnumerable<Group>> SearchGroupsAsync(string searchTerm);
        Task<bool> JoinGroupAsync(Guid userId, Guid groupId);
        Task<bool> LeaveGroupAsync(Guid userId, Guid groupId);
        Task<bool> AddMemberAsync(Guid adminId, Guid groupId, Guid targetUserId);
        Task<bool> RemoveMemberAsync(Guid adminId, Guid groupId, Guid targetUserId);
        Task<IEnumerable<User>> GetGroupMembersAsync(Guid groupId);
        Task<bool> IsMemberAsync(Guid userId, Guid groupId);
        Task<bool> IsAdminAsync(Guid userId, Guid groupId);
        Task<Guid?> GetConversationIdAsync(Guid groupId);
        Task<IEnumerable<Group>> GetSuggestedGroupsAsync(Guid userId);
        Task<bool> InviteToGroupAsync(Guid inviterId, Guid groupId, Guid friendId);
        Task<IEnumerable<User>> GetUsersWithSameTopicAsync(Guid groupId);
        Task<bool> UpdateGroupAvatarAsync(Guid groupId, string avatarUrl);
    }
}
