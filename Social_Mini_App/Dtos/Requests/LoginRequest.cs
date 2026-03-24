using System.ComponentModel.DataAnnotations;
using Social_Mini_App.Messages;

namespace Social_Mini_App.Dtos.Requests
{
    public class LoginRequest
    {
        [Required(ErrorMessage = AuthMsg.Validation.UsernameRequired)]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = AuthMsg.Validation.PasswordRequired)]
        public string Password { get; set; } = string.Empty;
    }
}
