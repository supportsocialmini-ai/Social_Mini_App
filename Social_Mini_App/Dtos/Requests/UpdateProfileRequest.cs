using System.ComponentModel.DataAnnotations;
using Social_Mini_App.Messages;

namespace Social_Mini_App.Dtos.Requests
{
    public class UpdateProfileRequest
    {
        [Required(ErrorMessage = AuthMsg.Validation.FullNameRequired)]
        [RegularExpression(@"^[a-zA-ZÀ-ỹ\s]{2,100}$", ErrorMessage = AuthMsg.Validation.FullNameInvalid)]
        [StringLength(100, MinimumLength = 2, ErrorMessage = AuthMsg.Validation.FullNameLength)]
        public string FullName { get; set; } = string.Empty;

        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = AuthMsg.Validation.EmailInvalid)]
        [MaxLength(255)]
        public string? Email { get; set; }
        public string? AvatarUrl { get; set; }

        [MaxLength(255, ErrorMessage = UserMsg.Validation.BioTooLong)]
        public string? Bio { get; set; }
    }
}
