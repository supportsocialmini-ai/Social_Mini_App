using System.ComponentModel.DataAnnotations;
using Social_Mini_App.Messages;

namespace Social_Mini_App.Dtos.Requests
{
    public class PostUploadDto
    {
        [Required(ErrorMessage = PostMsg.Validation.ContentRequired)]
        [MaxLength(1000, ErrorMessage = PostMsg.Validation.PostTooLong)]
        public string Content { get; set; } = string.Empty;
        public string Privacy { get; set; } = "Public";
        public IFormFile? ImageFile { get; set; }
    } 
}
