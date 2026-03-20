using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Social_Mini_App.Interfaces;
using System.Security.Claims;

namespace Social_Mini_App.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notifService;
        public NotificationController(INotificationService notifService) => _notifService = notifService;

        // 1. LẤY DANH SÁCH THÔNG BÁO CỦA TÔI
        [HttpGet]
        public async Task<IActionResult> GetMyNotifications()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdStr == null || !Guid.TryParse(userIdStr, out var userId)) 
                return Unauthorized();

            var notifications = await _notifService.GetNotificationsAsync(userId);
            return Ok(notifications);
        }

        // 2. ĐÁNH DẤU ĐÃ ĐỌC
        [HttpPost("{id}/read")]
        public async Task<IActionResult> MarkAsRead(Guid id)
        {
            if (await _notifService.MarkAsReadAsync(id)) return Ok();
            return BadRequest();
        }

        [HttpPost("read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdStr == null || !Guid.TryParse(userIdStr, out var userId)) 
                return Unauthorized();

            if (await _notifService.MarkAllAsReadAsync(userId)) return Ok();
            return BadRequest();
        }
    }
}