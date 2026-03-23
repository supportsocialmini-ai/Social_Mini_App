using System.ComponentModel.DataAnnotations;
using Social_Mini_App.Constants;

namespace Social_Mini_App.Dtos.Requests
{
    public class ChangePasswordRequest
    {
        [Required(ErrorMessage = "Mật khẩu cũ không được để trống!")]
        public string OldPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = ValidatorMessages.PasswordRequired)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{6,50}$", ErrorMessage = ValidatorMessages.PasswordInvalid)]
        [StringLength(50, MinimumLength = 6, ErrorMessage = ValidatorMessages.PasswordLength)]
        public string NewPassword { get; set; } = string.Empty;
    }
}
