using System.ComponentModel.DataAnnotations;
using Social_Mini_App.Constants;

namespace Social_Mini_App.Dtos.Requests
{
    public class RegisterRequest
    {
        [Required(ErrorMessage = ValidatorMessages.UsernameRequired)]
        [RegularExpression(@"^[a-z0-9_]{4,30}$", ErrorMessage = ValidatorMessages.UsernameInvalid)]
        [StringLength(30, MinimumLength = 4, ErrorMessage = ValidatorMessages.UsernameLength)]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = ValidatorMessages.FullNameRequired)]
        [RegularExpression(@"^[a-zA-ZÀ-ỹ\s]{2,100}$", ErrorMessage = ValidatorMessages.FullNameInvalid)]
        [StringLength(100, MinimumLength = 2, ErrorMessage = ValidatorMessages.FullNameLength)]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = ValidatorMessages.EmailRequired)]
        [RegularExpression(@"^[^\s@]+@[^\s@]+\.[^\s@]+$", ErrorMessage = ValidatorMessages.EmailInvalid)]
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = ValidatorMessages.PasswordRequired)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{6,50}$", ErrorMessage = ValidatorMessages.PasswordInvalid)]
        [StringLength(50, MinimumLength = 6, ErrorMessage = ValidatorMessages.PasswordLength)]
        public string Password { get; set; } = string.Empty;
    }
}
