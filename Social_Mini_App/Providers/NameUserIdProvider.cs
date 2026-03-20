using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

public class NameUserIdProvider : IUserIdProvider
{
    public string GetUserId(HubConnectionContext connection)
    {
        // Nó sẽ lấy đúng cái ID mà mày đã lưu vào Token lúc đăng nhập
        return connection.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
    }
}