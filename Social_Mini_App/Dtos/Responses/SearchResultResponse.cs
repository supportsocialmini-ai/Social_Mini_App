namespace Social_Mini_App.Dtos.Responses
{
    public class PostSearchDto
    {
        public Guid PostId { get; set; }
        public string PostContent { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public Guid AuthorId { get; set; }
        public DateTime CreatedAt { get; set; }
        public int LikeCount { get; set; }
        public int CommentCount { get; set; }
    }

    public class GroupSearchDto
    {
        public Guid GroupId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? AvatarUrl { get; set; }
        public string Privacy { get; set; } = "Public";
        public string? Category { get; set; }
        public int MemberCount { get; set; }
    }

    public class SearchResultResponse
    {
        public List<UserSummaryDto> Users { get; set; } = new();
        public List<PostSearchDto> Posts { get; set; } = new();
        public List<GroupSearchDto> Groups { get; set; } = new();
    }
}
