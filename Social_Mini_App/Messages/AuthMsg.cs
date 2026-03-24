namespace Social_Mini_App.Messages;

public static class AuthMsg
{
    public struct Register
    {
        public const string Success = "Auth.Register.Success";
        public const string UserExists = "Auth.Register.UserExists";
        public const string EmailExists = "Auth.Register.EmailExists";
    }

    public struct Login
    {
        public const string Fail = "Auth.Login.Fail";
        public const string UserNotVerified = "Auth.Login.UserNotVerified";
    }

    public struct Verify
    {
        public const string Success = "Auth.Verify.Success";
        public const string Fail = "Auth.Verify.Fail";
    }

    public struct Password
    {
        public const string ChangeSuccess = "Auth.Password.ChangeSuccess";
        public const string ChangeFail = "Auth.Password.ChangeFail";
        public const string VerifySuccess = "Auth.Password.VerifySuccess";
        public const string VerifyFail = "Auth.Password.VerifyFail";
    }

    public struct Validation
    {
        public const string FullNameRequired = "FullNameRequired";
        public const string FullNameInvalid = "FullNameInvalid";
        public const string FullNameLength = "FullNameLength";
        public const string EmailRequired = "EmailRequired";
        public const string EmailInvalid = "EmailInvalid";
        public const string UsernameRequired = "UsernameRequired";
        public const string UsernameInvalid = "UsernameInvalid";
        public const string UsernameLength = "UsernameLength";
        public const string PasswordRequired = "PasswordRequired";
        public const string PasswordInvalid = "PasswordInvalid";
        public const string PasswordLength = "PasswordLength";
    }
}
