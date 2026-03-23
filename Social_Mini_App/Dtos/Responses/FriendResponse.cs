namespace Social_Mini_App.Dtos.Responses
{
    public class UserSummaryDto
    {
        public Guid UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? AvatarUrl { get; set; }
        public string? Bio { get; set; }
    }

    public class FriendRequestDto
    {
        public Guid FriendId { get; set; }
        public Guid RequesterId { get; set; }
        public string RequesterName { get; set; } = string.Empty;
        public string? RequesterUsername { get; set; }
        public string? RequesterAvatar { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime CreatedAt { get; set; }
    }
}
