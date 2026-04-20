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
                        path.Contains("/api/admin/maintenance-status") ||
                        path.StartsWith("/chathub")
                    );

                    if (!isAdmin && !isAllowedPath)
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsync("{\"message\": \"Hệ thống đang bảo trì. Vui lòng quay lại sau!\", \"isMaintenance\": true}");
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
