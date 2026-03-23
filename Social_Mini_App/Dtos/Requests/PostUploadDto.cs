using System.ComponentModel.DataAnnotations;
using Social_Mini_App.Constants;

namespace Social_Mini_App.Dtos.Requests
{
    public class PostUploadDto
    {
        [Required(ErrorMessage = ValidatorMessages.ContentRequired)]
        [MaxLength(5000, ErrorMessage = ValidatorMessages.PostTooLong)]
        public string Content { get; set; } = string.Empty;
        public string Privacy { get; set; } = "Public";
        public IFormFile? ImageFile { get; set; }
    }
}
