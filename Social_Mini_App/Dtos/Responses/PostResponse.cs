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
        public string Privacy { get; set; } = "Public";
        public int LikeCount { get; set; } // Tổng số like
        public bool IsLiked { get; set; } // Thằng đang xem đã like chưa?
        public bool IsFriend { get; set; } // Đã là bạn bè chưa?
        public bool IsSponsored { get; set; } // Bài viết có đang được quảng cáo không?
        public DateTime? SponsorEndDate { get; set; }
        public string? FirstLikeName { get; set; }
        public int CommentCount { get; set; }

        // Mở rộng cho chức năng Share
        public bool IsShare { get; set; }
        public Guid? ShareId { get; set; }
        public string? ShareContent { get; set; }
        public Guid? OriginalPostId { get; set; }
        public PostResponse? OriginalPost { get; set; }

        public Guid? GroupId { get; set; }
        public string? GroupName { get; set; }

        public bool IsViolated { get; set; }
        public string? ViolationReason { get; set; }
        public bool IsAppealed { get; set; }
        public string? AppealReason { get; set; }
    }
}
