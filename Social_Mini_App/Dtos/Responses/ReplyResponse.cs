namespace Social_Mini_App.Dtos.Responses
{
    public class ReplyResponse
    {
        public Guid ReplyId { get; set; }
        public Guid CommentId { get; set; }
        public Guid UserId { get; set; }
        public string ReplyContent { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        // Thông tin người phản hồi
        public string FullName { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
    }
}
