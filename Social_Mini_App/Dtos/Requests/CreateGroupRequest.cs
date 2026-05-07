namespace Social_Mini_App.Dtos.Requests;

public class CreateGroupRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Privacy { get; set; } = "Public"; // Public, Private
    public string? Category { get; set; }
    public List<Guid> MemberIds { get; set; } = new();
}
