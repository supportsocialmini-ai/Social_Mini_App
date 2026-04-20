using Social_Mini_App.Dtos.Responses;
using Social_Mini_App.Models;

public interface ICommentService
{
    Task<List<CommentResponse>> GetCommentsByPostIdAsync(Guid postId);
    Task<CommentResponse?> CreateCommentAsync(Comment comment);
    Task<bool> DeleteCommentAsync(Guid commentId, Guid userId);

    // Reply Methods
    Task<ReplyResponse?> CreateReplyAsync(Reply reply);
    Task<bool> DeleteReplyAsync(Guid replyId, Guid userId);
}