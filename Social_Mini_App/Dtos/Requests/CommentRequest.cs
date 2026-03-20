namespace Social_Mini_App.Dtos.Requests
{
    public class CommentRequest
    {
        public Guid PostId { get; set; }
        public string Content { get; set; } = string.Empty;
        public Guid? ParentCommentId { get; set; }
    }
}
