namespace Social_Mini_App.Messages;

public static class UserMsg
{
    public struct Profile
    {
        public const string UpdateSuccess = "User.Profile.UpdateSuccess";
        public const string UpdateFail = "User.Profile.UpdateFail";
        public const string NotFound = "User.Profile.NotFound";
        public const string Unauthorized = "User.Profile.Unauthorized";
    }

    public struct Avatar
    {
        public const string UploadSuccess = "User.Avatar.UploadSuccess";
        public const string UploadFail = "User.Avatar.UploadFail";
        public const string FileRequired = "User.Avatar.FileRequired";
        public const string InvalidType = "User.Avatar.InvalidType";
    }

    public struct Validation
    {
        public const string BioTooLong = "BioTooLong";
    }
}
