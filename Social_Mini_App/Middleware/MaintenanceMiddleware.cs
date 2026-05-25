using Microsoft.EntityFrameworkCore;
using MiniSocialNetwork.Data;
using System.Net;

namespace Social_Mini_App.Middleware
{
    public class MaintenanceMiddleware
    {
        private readonly RequestDelegate _next;

        public MaintenanceMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, DataContext dbContext)
        {
            try 
            {
                // Lấy trạng thái bảo trì từ DB
                var maintenanceSetting = await dbContext.SystemSettings
                    .FirstOrDefaultAsync(s => s.Key == "MaintenanceMode");

                bool isMaintenance = maintenanceSetting?.Value?.ToLower() == "true";

                if (isMaintenance)
                {
                    // Kiểm tra xem user có phải admin không
                    bool isAdmin = context.User.Identity?.IsAuthenticated == true && 
                                   context.User.IsInRole("Admin");

                    // Cho phép Admin truy cập mọi nơi
                    var path = context.Request.Path.Value?.ToLower();
                    bool isAllowedPath = path != null && (
                        path.Contains("/api/auth/login") || 
                        path.Contains("/api/auth/register") || 
                        path.Contains("/api/admin/maintenance-status") ||
                        path.Contains("/api/admin/maintenance-info") ||
                        path.Contains("/api/system/ping") ||
                        path.StartsWith("/chathub")
                    );

                    if (!isAdmin && !isAllowedPath)
                    {
                        var reasonSetting = await dbContext.SystemSettings.FirstOrDefaultAsync(s => s.Key == "MaintenanceReason");
                        var versionSetting = await dbContext.SystemSettings.FirstOrDefaultAsync(s => s.Key == "MaintenanceVersion");
                        var endTimeSetting = await dbContext.SystemSettings.FirstOrDefaultAsync(s => s.Key == "MaintenanceEndTime");

                        var reason = reasonSetting?.Value ?? "";
                        var version = versionSetting?.Value ?? "";
                        var endTime = endTimeSetting?.Value ?? "";

                        context.Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsync(
                            $"{{\"message\": \"H\u1ec7 th\u1ed1ng \u0111ang b\u1ea3o tr\u00ec. Vui l\u00f2ng quay l\u1ea1i sau!\", \"isMaintenance\": true, \"reason\": \"{reason}\", \"version\": \"{version}\", \"endTime\": \"{endTime}\"}}"
                        );
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                // Nếu lỗi database (ví dụ bảng chưa tồn tại), cứ cho qua để web không bị sập 500
                Console.WriteLine($"[MaintenanceMiddleware] Error checking status: {ex.Message}");
            }

            await _next(context);
        }
    }
}
