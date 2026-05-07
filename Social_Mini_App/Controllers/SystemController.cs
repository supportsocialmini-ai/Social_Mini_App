using Microsoft.AspNetCore.Mvc;

namespace Social_Mini_App.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SystemController : ControllerBase
    {
        // Endpoint dùng để ping giữ cho server Render không bị ngủ
        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok(new { 
                status = "Alive", 
                timestamp = DateTime.UtcNow,
                message = "System is running normally" 
            });
        }
    }
}
