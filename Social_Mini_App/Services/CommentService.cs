using Microsoft.EntityFrameworkCore;
using MiniSocialNetwork.Data;
using Social_Mini_App.Dtos.Responses;
using Social_Mini_App.Interfaces;
using Social_Mini_App.Models;

namespace Social_Mini_App.Services
{
    public class CommentService : ICommentService
    {
        private readonly DataContext _context;
        public CommentService(DataContext context) => _context = context;

        // 1. LẤY DANH SÁCH BÌNH LUẬN THEO POST ID (Hỗ trợ cấu trúc lồng nhau)
        public async Task<List<CommentResponse>> GetCommentsByPostIdAsync(Guid postId)
        {
            var allComments = await _context.Comments
                .Where(c => c.PostId == postId)
                .Include(c => c.User)
                .OrderBy(c => c.CreatedAt)
                .Select(c => new CommentResponse
                {
                    CommentId = c.CommentId,
                    PostId = c.PostId,
                    UserId = c.UserId,
                    CommentContent = c.CommentContent,
                    CreatedAt = c.CreatedAt,
                    FullName = c.User!.FullName ?? c.User.Username,
                    AvatarUrl = c.User.AvatarUrl,
                    ParentCommentId = c.ParentCommentId
                })
                .ToListAsync();

            // Xây dựng cấu trúc cây
            var commentMap = allComments.ToDictionary(c => c.CommentId);
            var rootComments = new List<CommentResponse>();

            foreach (var comment in allComments)
            {
                if (comment.ParentCommentId == null)
                {
                    rootComments.Add(comment);
                }
                else
                {
                    if (commentMap.TryGetValue(comment.ParentCommentId.Value, out var parent))
                    {
                        parent.Replies.Add(comment);
                    }
                }
            }

            return rootComments;
        }
 
        // 2. TẠO BÌNH LUẬN MỚI
        public async Task<CommentResponse?> CreateCommentAsync(Comment comment)
        {
            _context.Comments.Add(comment);
            if (await _context.SaveChangesAsync() > 0)
            {
                // Sau khi lưu xong, lấy lại thông tin kèm User để trả về cho FE
                var result = await _context.Comments
                    .Include(c => c.User)
                    .FirstOrDefaultAsync(c => c.CommentId == comment.CommentId);
 
                if (result == null) return null;
 
                return new CommentResponse
                {
                    CommentId = result.CommentId,
                    PostId = result.PostId,
                    UserId = result.UserId,
                    CommentContent = result.CommentContent,
                    CreatedAt = result.CreatedAt,
                    FullName = result.User!.FullName ?? result.User.Username,
                    AvatarUrl = result.User.AvatarUrl,
                    ParentCommentId = result.ParentCommentId
                };
            }
            return null;
        }
 
        // 3. XÓA BÌNH LUẬN (Phải đúng chủ nhân mới được xóa)
        public async Task<bool> DeleteCommentAsync(Guid commentId, Guid userId)
        {
            var comment = await _context.Comments.FindAsync(commentId);

            if (comment == null) return false;

            // Kiểm tra xem thằng đang yêu cầu xóa có phải là thằng đã viết cmt không
            if (comment.UserId != userId) return false;

            _context.Comments.Remove(comment);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}