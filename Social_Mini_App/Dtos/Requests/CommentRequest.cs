using System.ComponentModel.DataAnnotations;
using Social_Mini_App.Constants;

namespace Social_Mini_App.Dtos.Requests
{
    public class CommentRequest
    {
        public Guid PostId { get; set; }

        [Required(ErrorMessage = ValidatorMessages.ContentRequired)]
        [MaxLength(1000, ErrorMessage = ValidatorMessages.CommentTooLong)]
        public string Content { get; set; } = string.Empty;
        public Guid? ParentCommentId { get; set; }
    }
}
