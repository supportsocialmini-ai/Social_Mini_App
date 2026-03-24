using System.ComponentModel.DataAnnotations;
using Social_Mini_App.Messages;

namespace Social_Mini_App.Dtos.Requests
{
    public class RegisterRequest
    {
        [Required(ErrorMessage = AuthMsg.Validation.UsernameRequired)]
        [RegularExpression(@"^[a-z0-9_]{4,30}$", ErrorMessage = AuthMsg.Validation.UsernameInvalid)]
        [StringLength(30, MinimumLength = 4, ErrorMessage = AuthMsg.Validation.UsernameLength)]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = AuthMsg.Validation.FullNameRequired)]
        [RegularExpression(@"^[a-zA-ZÀ-ỹ\s]{2,100}$", ErrorMessage = AuthMsg.Validation.FullNameInvalid)]
        [StringLength(100, MinimumLength = 2, ErrorMessage = AuthMsg.Validation.FullNameLength)]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = AuthMsg.Validation.EmailRequired)]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = AuthMsg.Validation.EmailInvalid)]
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = AuthMsg.Validation.PasswordRequired)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{6,50}$", ErrorMessage = AuthMsg.Validation.PasswordInvalid)]
        [StringLength(50, MinimumLength = 6, ErrorMessage = AuthMsg.Validation.PasswordLength)]
        public string Password { get; set; } = string.Empty;
    }
}
