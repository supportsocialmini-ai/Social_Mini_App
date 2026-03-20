using Social_Mini_App.Dtos.Responses;

namespace Social_Mini_App.Interfaces
{
    public interface IFriendService
    {
        Task<bool> SendFriendRequestAsync(Guid requesterId, Guid addresseeId);
        Task<bool> AcceptFriendRequestAsync(Guid requestId, Guid userId);
        Task<bool> DeclineFriendRequestAsync(Guid requestId, Guid userId);
        Task<bool> UnfriendAsync(Guid userId, Guid friendId);
        Task<bool> CancelFriendRequestAsync(Guid userId, Guid requestId);
        Task<List<UserSummaryDto>> GetFriendsAsync(Guid userId);
        Task<List<FriendRequestDto>> GetPendingRequestsAsync(Guid userId);
        Task<(string Status, Guid? RequestId)> GetFriendshipStatusAsync(Guid userId1, Guid userId2);
    }
}
