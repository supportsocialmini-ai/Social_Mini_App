namespace Social_Mini_App.Dtos.Requests;

public class CreateGroupRequest
{
    public string Name { get; set; } = string.Empty;
    public List<Guid> MemberIds { get; set; } = new();
}
