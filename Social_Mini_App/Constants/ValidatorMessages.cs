namespace Social_Mini_App.Constants
{
    public static class ValidatorMessages
    {
        public const string PostTooLong = "Bài viết vượt quá giới hạn 5000 ký tự rồi bạn ơi!";
        public const string CommentTooLong = "Bình luận tối đa 1000 ký tự thôi nè!";
        public const string MessageTooLong = "Tin nhắn dài quá, tối đa 2000 ký tự nhé!";
        public const string ContentRequired = "Nội dung không được để trống đâu đó!";
        
        // FullName
        public const string FullNameRequired = "Họ và tên không được để trống";
        public const string FullNameInvalid = "Họ và tên chỉ được chứa chữ cái và khoảng trắng";
        public const string FullNameLength = "Họ và tên phải từ 2 đến 100 ký tự";

        // Email
        public const string EmailRequired = "Email không được để trống";
        public const string EmailInvalid = "Email không đúng định dạng";
        public const string EmailExists = "Email đã tồn tại";

        // Username
        public const string UsernameRequired = "Username không được để trống";
        public const string UsernameInvalid = "Username chỉ gồm chữ thường, số và dấu _";
        public const string UsernameLength = "Username phải từ 4 đến 30 ký tự";
        public const string UsernameExists = "Username đã tồn tại";

        // Password
        public const string PasswordRequired = "Mật khẩu không được để trống";
        public const string PasswordInvalid = "Mật khẩu phải có ít nhất 1 chữ hoa, 1 chữ thường và 1 số";
        public const string PasswordLength = "Mật khẩu phải từ 6 đến 50 ký tự";

        // Other
        public const string BioTooLong = "Tiểu sử giới hạn trong 255 ký tự thôi bạn ơi!";
    }
}
