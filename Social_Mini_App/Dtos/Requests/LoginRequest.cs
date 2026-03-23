using System.ComponentModel.DataAnnotations;
using Social_Mini_App.Constants;

namespace Social_Mini_App.Dtos.Requests
{
    public class LoginRequest
    {
        [Required(ErrorMessage = ValidatorMessages.UsernameRequired)]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = ValidatorMessages.PasswordRequired)]
        public string Password { get; set; } = string.Empty;
    }
}
