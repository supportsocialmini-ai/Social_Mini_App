namespace Social_Mini_App.Messages;

public static class ChatMsg
{
    public struct Action
    {
        public const string ReadSuccess = "Chat.Action.ReadSuccess";
    }

    public struct Upload
    {
        public const string Fail = "Chat.Upload.Fail";
    }
}
