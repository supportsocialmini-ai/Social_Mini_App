namespace Social_Mini_App.Messages;

public static class PostMsg
{
    public struct Upsert
    {
        public const string CreateSuccess = "Post.Upsert.CreateSuccess";
        public const string CreateFail = "Post.Upsert.CreateFail";
        public const string UpdateSuccess = "Post.Upsert.UpdateSuccess";
        public const string UpdateFail = "Post.Upsert.UpdateFail";
        public const string ImageUploadFail = "Post.Upsert.ImageUploadFail";
    }

    public struct Get
    {
        public const string NotFound = "Post.Get.NotFound";
    }

    public struct Delete
    {
        public const string Success = "Post.Delete.Success";
        public const string Fail = "Post.Delete.Fail";
    }

    public struct Validation
    {
        public const string ContentRequired = "ContentRequired";
        public const string PostTooLong = "PostTooLong";
    }
}
