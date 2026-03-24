using System.ComponentModel.DataAnnotations;
using Social_Mini_App.Messages;

namespace Social_Mini_App.Dtos.Requests
{
    public class CommentRequest
    {
        public Guid PostId { get; set; }

        [Required(ErrorMessage = CommentMsg.Validation.ContentRequired)]
        [MaxLength(200, ErrorMessage = CommentMsg.Validation.CommentTooLong)]
        public string Content { get; set; } = string.Empty;
        public Guid? ParentCommentId { get; set; }
    }
}
