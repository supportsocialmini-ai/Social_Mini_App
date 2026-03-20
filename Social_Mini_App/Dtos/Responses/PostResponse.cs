namespace Social_Mini_App.Dtos.Responses
{
    public class PostResponse
    {
        public Guid PostId { get; set; }
        public string PostContent { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public Guid UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public string? ImageUrl { get; set; } // Ảnh bài viết
        public int LikeCount { get; set; } // Tổng số like
        public bool IsLiked { get; set; } // Thằng đang xem đã like chưa?
        public string? FirstLikeName { get; set; }
        public int CommentCount { get; set; }
    }
}
