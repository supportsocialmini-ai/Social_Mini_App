using System.ComponentModel.DataAnnotations;
using Social_Mini_App.Messages;

namespace Social_Mini_App.Dtos.Requests
{
    public class ChangePasswordRequest
    {
        [Required(ErrorMessage = AuthMsg.Validation.PasswordRequired)]
        public string OldPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = AuthMsg.Validation.PasswordRequired)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{6,50}$", ErrorMessage = AuthMsg.Validation.PasswordInvalid)]
        [StringLength(50, MinimumLength = 6, ErrorMessage = AuthMsg.Validation.PasswordLength)]
        public string NewPassword { get; set; } = string.Empty;
    }
}
