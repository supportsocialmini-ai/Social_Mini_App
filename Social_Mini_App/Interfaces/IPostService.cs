using Social_Mini_App.Models;
using Social_Mini_App.Dtos.Responses;

namespace Social_Mini_App.Interfaces
{
    public interface IPostService
    {
        Task<List<PostResponse>> GetNewsfeedAsync(Guid currentUserId);
        Task<Post?> GetPostByIdAsync(Guid id);
        Task<bool> CreatePostAsync(Post post);
        Task<bool> UpdatePostAsync(Post post);
        Task<bool> DeletePostAsync(Guid id);
        Task<List<PostResponse>> GetMyPostsAsync(Guid userId, Guid currentUserId);
        Task<List<PostResponse>> GetPostsByUserIdAsync(Guid userId, Guid currentUserId);
        Task<List<UserSummaryDto>> GetPostLikesAsync(Guid postId);
    }
}
