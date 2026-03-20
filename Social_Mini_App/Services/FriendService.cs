using MiniSocialNetwork.Data;
using Microsoft.EntityFrameworkCore;
using Social_Mini_App.Dtos.Responses;
using Social_Mini_App.Interfaces;
using Social_Mini_App.Models;

namespace Social_Mini_App.Services
{
    public class FriendService : IFriendService
    {
        private readonly DataContext _context;
        private readonly INotificationService _notifService;
        
        public FriendService(DataContext context, INotificationService notifService)
        {
            _context = context;
            _notifService = notifService;
        }

        public async Task<bool> SendFriendRequestAsync(Guid requesterId, Guid addresseeId)
        {
            if (requesterId == addresseeId) return false;

            // Check if friendship already exists
            var existingFriendshipMember = await _context.FriendshipMembers
                .Where(fm => fm.UserId == requesterId)
                .Select(fm => fm.FriendshipId)
                .FirstOrDefaultAsync(fid => _context.FriendshipMembers.Any(fm2 => fm2.FriendshipId == fid && fm2.UserId == addresseeId));

            if (existingFriendshipMember != Guid.Empty) return false;

            var friendship = new Friendship
            {
                FriendshipId = Guid.NewGuid(),
                Status = "Pending",
                CreatedAt = DateTime.Now,
                RequestedAt = DateTime.Now
            };

            var members = new List<FriendshipMember>
            {
                new FriendshipMember { MemberId = Guid.NewGuid(), FriendshipId = friendship.FriendshipId, UserId = requesterId, IsRequestSender = true },
                new FriendshipMember { MemberId = Guid.NewGuid(), FriendshipId = friendship.FriendshipId, UserId = addresseeId, IsRequestSender = false }
            };

            await _context.Friendships.AddAsync(friendship);
            await _context.FriendshipMembers.AddRangeAsync(members);
            
            var result = await _context.SaveChangesAsync() > 0;

            if (result)
            {
                await _notifService.CreateNotifAsync(requesterId, addresseeId, null, "FriendRequest");
            }

            return result;
        }

        public async Task<bool> AcceptFriendRequestAsync(Guid requestId, Guid userId)
        {
            var friendship = await _context.Friendships
                .Include(f => f.Members)
                .FirstOrDefaultAsync(f => f.FriendshipId == requestId);

            if (friendship == null || friendship.Status != "Pending") return false;

            // Check if user is the addressee (not the requester)
            var userMember = friendship.Members.FirstOrDefault(m => m.UserId == userId);
            if (userMember == null || userMember.IsRequestSender) return false;

            friendship.Status = "Accepted";
            friendship.AcceptedAt = DateTime.Now;
            var result = await _context.SaveChangesAsync() > 0;

            if (result)
            {
                var requesterMember = friendship.Members.First(m => m.IsRequestSender);
                await _notifService.CreateNotifAsync(userId, requesterMember.UserId, null, "FriendAccept");
            }

            return result;
        }

        public async Task<bool> DeclineFriendRequestAsync(Guid requestId, Guid userId)
        {
            var friendship = await _context.Friendships
                .Include(f => f.Members)
                .FirstOrDefaultAsync(f => f.FriendshipId == requestId);

            if (friendship == null || friendship.Status != "Pending") return false;

            var userMember = friendship.Members.FirstOrDefault(m => m.UserId == userId);
            if (userMember == null || userMember.IsRequestSender) return false;

            _context.Friendships.Remove(friendship);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UnfriendAsync(Guid userId, Guid friendId)
        {
            var friendshipId = await _context.FriendshipMembers
                .Where(fm => fm.UserId == userId)
                .Select(fm => fm.FriendshipId)
                .FirstOrDefaultAsync(fid => _context.FriendshipMembers.Any(fm2 => fm2.FriendshipId == fid && fm2.UserId == friendId));

            if (friendshipId == Guid.Empty) return false;

            var friendship = await _context.Friendships.FindAsync(friendshipId);
            if (friendship == null || friendship.Status != "Accepted") return false;

            _context.Friendships.Remove(friendship);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> CancelFriendRequestAsync(Guid userId, Guid requestId)
        {
            var friendship = await _context.Friendships
                .Include(f => f.Members)
                .FirstOrDefaultAsync(f => f.FriendshipId == requestId);

            if (friendship == null || friendship.Status != "Pending") return false;

            var userMember = friendship.Members.FirstOrDefault(m => m.UserId == userId);
            if (userMember == null || !userMember.IsRequestSender) return false;

            _context.Friendships.Remove(friendship);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<List<UserSummaryDto>> GetFriendsAsync(Guid userId)
        {
            return await _context.FriendshipMembers
                .Where(fm => fm.UserId == userId)
                .Join(_context.Friendships.Where(f => f.Status == "Accepted"),
                      fm => fm.FriendshipId,
                      f => f.FriendshipId,
                      (fm, f) => f.FriendshipId)
                .SelectMany(fid => _context.FriendshipMembers
                    .Where(fm2 => fm2.FriendshipId == fid && fm2.UserId != userId)
                    .Select(fm2 => new UserSummaryDto
                    {
                        UserId = fm2.User.UserId,
                        Username = fm2.User.Username,
                        FullName = fm2.User.FullName,
                        AvatarUrl = fm2.User.AvatarUrl
                    }))
                .ToListAsync();
        }

        public async Task<List<FriendRequestDto>> GetPendingRequestsAsync(Guid userId)
        {
            return await _context.FriendshipMembers
                .Where(fm => fm.UserId == userId && !fm.IsRequestSender)
                .Join(_context.Friendships.Where(f => f.Status == "Pending"),
                      fm => fm.FriendshipId,
                      f => f.FriendshipId,
                      (fm, f) => f)
                .Select(f => new
                {
                    f.FriendshipId,
                    f.Status,
                    f.CreatedAt,
                    Requester = f.Members.Where(m => m.IsRequestSender).Select(m => m.User).FirstOrDefault()
                })
                .Select(x => new FriendRequestDto
                {
                    FriendId = x.FriendshipId,
                    RequesterId = x.Requester!.UserId,
                    RequesterName = x.Requester.FullName ?? x.Requester.Username,
                    RequesterUsername = x.Requester.Username,
                    RequesterAvatar = x.Requester.AvatarUrl,
                    Status = x.Status,
                    CreatedAt = x.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<(string Status, Guid? RequestId)> GetFriendshipStatusAsync(Guid userId1, Guid userId2)
        {
            if (userId1 == userId2) return ("Self", null);

            var friendshipMember = await _context.FriendshipMembers
                .Where(fm => fm.UserId == userId1)
                .Join(_context.FriendshipMembers.Where(fm2 => fm2.UserId == userId2),
                      fm => fm.FriendshipId,
                      fm2 => fm2.FriendshipId,
                      (fm, fm2) => new { fm.FriendshipId, fm.IsRequestSender })
                .FirstOrDefaultAsync();

            if (friendshipMember == null) return ("None", null);

            var friendship = await _context.Friendships.FindAsync(friendshipMember.FriendshipId);
            if (friendship == null) return ("None", null);

            if (friendship.Status == "Accepted") return ("Accepted", friendship.FriendshipId);
            if (friendshipMember.IsRequestSender) return ("Sent", friendship.FriendshipId);
            return ("Received", friendship.FriendshipId);
        }
    }
}
