using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniSocialNetwork.Wrappers;
using Social_Mini_App.Dtos;
using Social_Mini_App.Interfaces;
using System.Security.Claims;

namespace Social_Mini_App.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ReportController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportController(IReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateReport([FromBody] ReportCreateRequest request)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Unauthorized(ApiResponse<string>.Fail("Unauthorized"));

            var result = await _reportService.CreateReportAsync(userId, request);
            if (result)
                return Ok(ApiResponse<string>.Ok("Báo cáo của bạn đã được gửi. Cảm ơn bạn đã giúp cộng đồng sạch đẹp hơn!"));

            return BadRequest(ApiResponse<string>.Fail("Không thể gửi báo cáo lúc này. Vui lòng thử lại sau."));
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("admin/list")]
        public async Task<IActionResult> GetAllReports()
        {
            var reports = await _reportService.GetAllReportsAsync();
            return Ok(ApiResponse<List<ReportResponse>>.Ok(reports));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("admin/resolve/{reportId}")]
        public async Task<IActionResult> ResolveReport(Guid reportId, [FromQuery] string action)
        {
            var adminIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(adminIdStr, out var adminId))
                return Unauthorized(ApiResponse<string>.Fail("Unauthorized"));

            if (action != "Resolved" && action != "Dismissed")
                return BadRequest(ApiResponse<string>.Fail("Hành động không hợp lệ."));

            var result = await _reportService.ResolveReportAsync(adminId, reportId, action);
            if (result)
                return Ok(ApiResponse<string>.Ok($"Đã xử lý báo cáo thành công: {action}"));

            return BadRequest(ApiResponse<string>.Fail("Không thể xử lý báo cáo này."));
        }
    }
}
