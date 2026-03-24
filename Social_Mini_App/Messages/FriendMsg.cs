namespace Social_Mini_App.Messages;

public static class FriendMsg
{
    public struct Request
    {
        public const string Success = "Friend.Request.Success";
        public const string Fail = "Friend.Request.Fail";
    }

    public struct Action
    {
        public const string AcceptSuccess = "Friend.Action.AcceptSuccess";
        public const string AcceptFail = "Friend.Action.AcceptFail";
        public const string DeclineSuccess = "Friend.Action.DeclineSuccess";
        public const string DeclineFail = "Friend.Action.DeclineFail";
        public const string UnfriendSuccess = "Friend.Action.UnfriendSuccess";
        public const string UnfriendFail = "Friend.Action.UnfriendFail";
        public const string CancelSuccess = "Friend.Action.CancelSuccess";
        public const string CancelFail = "Friend.Action.CancelFail";
    }
}
