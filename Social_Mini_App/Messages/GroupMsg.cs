namespace Social_Mini_App.Messages;

public static class GroupMsg
{
    public struct Upsert
    {
        public const string CreateSuccess = "Group.Upsert.CreateSuccess";
        public const string CreateFail = "Group.Upsert.CreateFail";
        public const string UpdateSuccess = "Group.Upsert.UpdateSuccess";
        public const string UpdateFail = "Group.Upsert.UpdateFail";
    }

    public struct Get
    {
        public const string NotFound = "Group.Get.NotFound";
    }

    public struct Member
    {
        public const string JoinSuccess = "Group.Member.JoinSuccess";
        public const string JoinFail = "Group.Member.JoinFail";
        public const string LeaveSuccess = "Group.Member.LeaveSuccess";
        public const string LeaveFail = "Group.Member.LeaveFail";
        public const string AddSuccess = "Group.Member.AddSuccess";
        public const string AddFail = "Group.Member.AddFail";
        public const string RemoveSuccess = "Group.Member.RemoveSuccess";
        public const string RemoveFail = "Group.Member.RemoveFail";
        public const string Unauthorized = "Group.Member.Unauthorized";
    }
}
