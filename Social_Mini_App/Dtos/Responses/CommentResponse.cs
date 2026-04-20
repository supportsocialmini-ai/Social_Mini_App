namespace Social_Mini_App.Dtos.Responses
{
    public class CommentResponse
    {
        public Guid CommentId { get; set; }
        public Guid PostId { get; set; }
        public Guid UserId { get; set; }
        public string CommentContent { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        // Thông tin người bình luận để FE hiển thị
        public string FullName { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }

        // Quan hệ với các phản hồi (Level 2)
        public List<ReplyResponse> Replies { get; set; } = new List<ReplyResponse>();
    }
}