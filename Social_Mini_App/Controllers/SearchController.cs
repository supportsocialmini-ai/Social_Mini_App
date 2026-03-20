using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniSocialNetwork.Wrappers;
using Social_Mini_App.Dtos.Responses;
using Social_Mini_App.Interfaces;
using Social_Mini_App.Models;
using Social_Mini_App.Dtos;
using System.Security.Claims;

namespace Social_Mini_App.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class SearchController : ControllerBase
    {
        private readonly ISearchService _searchService;

        public SearchController(ISearchService searchService)
        {
            _searchService = searchService;
        }

        [HttpGet]
        public async Task<IActionResult> Search([FromQuery] string q)
        {
            if (string.IsNullOrWhiteSpace(q) || q.Trim().Length < 2)
                return BadRequest(ApiResponse<SearchResultResponse>.Fail("Từ khóa tìm kiếm phải ít nhất 2 ký tự!"));

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Guid currentUserId = Guid.Empty;
            if (userIdStr != null) Guid.TryParse(userIdStr, out currentUserId);

            var result = await _searchService.SearchAsync(q.Trim(), currentUserId);
            return Ok(ApiResponse<SearchResultResponse>.Ok(result));
        }
    }
}
