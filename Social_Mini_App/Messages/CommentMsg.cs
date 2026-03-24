namespace Social_Mini_App.Messages;

public static class CommentMsg
{
    public struct Upsert
    {
        public const string CreateFail = "Comment.Upsert.CreateFail";
    }

    public struct Delete
    {
        public const string Success = "Comment.Delete.Success";
        public const string Fail = "Comment.Delete.Fail";
    }

    public struct Validation
    {
        public const string ContentRequired = "ContentRequired";
        public const string CommentTooLong = "CommentTooLong";
    }
}
