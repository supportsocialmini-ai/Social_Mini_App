namespace Social_Mini_App.Messages;

public static class NotificationMsg
{
    public struct Action
    {
        public const string Like = "Notification.Action.Like";
        public const string Comment = "Notification.Action.Comment";
        public const string FriendRequest = "Notification.Action.FriendRequest";
        public const string FriendAccept = "Notification.Action.FriendAccept";
        public const string GroupInvite = "Notification.Action.GroupInvite";
        public const string Violation = "Notification.Action.Violation";
        public const string AppealAccept = "Notification.Action.AppealAccept";
        public const string AppealReject = "Notification.Action.AppealReject";
        public const string AppealSubmit = "Notification.Action.AppealSubmit";
    }
}
