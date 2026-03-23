using System.ComponentModel.DataAnnotations;
using Social_Mini_App.Constants;

namespace Social_Mini_App.Dtos.Requests
{
    public class UpdateProfileRequest
    {
        [Required(ErrorMessage = ValidatorMessages.FullNameRequired)]
        [RegularExpression(@"^[a-zA-ZÀ-ỹ\s]{2,100}$", ErrorMessage = ValidatorMessages.FullNameInvalid)]
        [StringLength(100, MinimumLength = 2, ErrorMessage = ValidatorMessages.FullNameLength)]
        public string FullName { get; set; } = string.Empty;

        [RegularExpression(@"^[^\s@]+@[^\s@]+\.[^\s@]+$", ErrorMessage = ValidatorMessages.EmailInvalid)]
        [MaxLength(255)]
        public string? Email { get; set; }
        public string? AvatarUrl { get; set; }

        [MaxLength(255, ErrorMessage = ValidatorMessages.BioTooLong)]
        public string? Bio { get; set; }
    }
}
