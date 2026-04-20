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
            // Cho phép truy cập vào trang Admin và tài khoản Admin
            // Lưu ý: Cần kiểm tra Role sau khi Authentication đã chạy.
            
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
                // Cho phép Guest truy cập Login/Register/Maintenance-status API
                var path = context.Request.Path.Value?.ToLower();
                bool isAllowedPath = path != null && (
                    path.Contains("/api/auth/login") || 
                    path.Contains("/api/admin/maintenance-status") ||
                    path.StartsWith("/chathub") // SignalR might need separate handling, but let's allow it for now
                );

                if (!isAdmin && !isAllowedPath)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync("{\"message\": \"Hệ thống đang bảo trì. Vui lòng quay lại sau!\", \"isMaintenance\": true}");
                    return;
                }
            }

            await _next(context);
        }
    }
}
