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

        // 1. LẤY DANH SÁCH BÌNH LUẬN THEO POST ID (Cấu trúc 2 tầng: Comment -> Replies)
        public async Task<List<CommentResponse>> GetCommentsByPostIdAsync(Guid postId)
        {
            return await _context.Comments
                .Where(c => c.PostId == postId)
                .Include(c => c.User)
                .Include(c => c.Replies.OrderBy(r => r.CreatedAt)) // Lấy luôn các câu trả lời
                    .ThenInclude(r => r.User)
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
                    Replies = c.Replies.Select(r => new ReplyResponse
                    {
                        ReplyId = r.ReplyId,
                        CommentId = r.CommentId,
                        UserId = r.UserId,
                        ReplyContent = r.ReplyContent,
                        CreatedAt = r.CreatedAt,
                        FullName = r.User!.FullName ?? r.User.Username,
                        AvatarUrl = r.User.AvatarUrl
                    }).ToList()
                })
                .ToListAsync();
        }

        // 2. TẠO BÌNH LUẬN MỚI
        public async Task<CommentResponse?> CreateCommentAsync(Comment comment)
        {
            _context.Comments.Add(comment);
            if (await _context.SaveChangesAsync() > 0)
            {
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
                    AvatarUrl = result.User.AvatarUrl
                };
            }
            return null;
        }

        // 3. XÓA BÌNH LUẬN
        public async Task<bool> DeleteCommentAsync(Guid commentId, Guid userId)
        {
            var comment = await _context.Comments.FindAsync(commentId);
            if (comment == null || comment.UserId != userId) return false;

            _context.Comments.Remove(comment);
            return await _context.SaveChangesAsync() > 0;
        }

        // 4. TẠO PHẢN HỒI (REPLY)
        public async Task<ReplyResponse?> CreateReplyAsync(Reply reply)
        {
            _context.Replies.Add(reply);
            if (await _context.SaveChangesAsync() > 0)
            {
                var result = await _context.Replies
                    .Include(r => r.User)
                    .FirstOrDefaultAsync(r => r.ReplyId == reply.ReplyId);

                if (result == null) return null;

                return new ReplyResponse
                {
                    ReplyId = result.ReplyId,
                    CommentId = result.CommentId,
                    UserId = result.UserId,
                    ReplyContent = result.ReplyContent,
                    CreatedAt = result.CreatedAt,
                    FullName = result.User!.FullName ?? result.User.Username,
                    AvatarUrl = result.User.AvatarUrl
                };
            }
            return null;
        }

        // 5. XÓA PHẢN HỒI
        public async Task<bool> DeleteReplyAsync(Guid replyId, Guid userId)
        {
            var reply = await _context.Replies.FindAsync(replyId);
            if (reply == null || reply.UserId != userId) return false;

            _context.Replies.Remove(reply);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}