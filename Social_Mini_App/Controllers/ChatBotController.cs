using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using System;

namespace Social_Mini_App.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Yêu cầu đăng nhập mới được xài Bot
    public class ChatBotController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;
        private static readonly HttpClient _httpClient = new HttpClient();

        public ChatBotController(IConfiguration configuration, IWebHostEnvironment env)
        {
            _configuration = configuration;
            _env = env;
        }

        [HttpPost("ask")]
        public async Task<IActionResult> AskBot([FromBody] ChatBotRequestDto request)
        {
            try
            {
                var apiKey = _configuration["Gemini:ApiKey"];
                if (string.IsNullOrEmpty(apiKey))
                {
                    return BadRequest("Chưa cấu hình API Key cho Gemini.");
                }

                // 1. Đọc System Prompt từ file BotKnowledge.txt
                var knowledgePath = Path.Combine(_env.ContentRootPath, "BotKnowledge.txt");
                var systemInstruction = "";
                if (System.IO.File.Exists(knowledgePath))
                {
                    systemInstruction = await System.IO.File.ReadAllTextAsync(knowledgePath);
                }

                // 2. Build Request Body theo chuẩn của Gemini API
                var requestBody = new
                {
                    system_instruction = new {
                        parts = new[] { new { text = systemInstruction } }
                    },
                    contents = new List<object>()
                };

                // Add History
                if (request.History != null)
                {
                    foreach (var msg in request.History)
                    {
                        requestBody.contents.Add(new
                        {
                            role = msg.Role == "bot" ? "model" : "user",
                            parts = new[] { new { text = msg.Content } }
                        });
                    }
                }

                // Add current message
                requestBody.contents.Add(new
                {
                    role = "user",
                    parts = new[] { new { text = request.Message } }
                });

                var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
                var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={apiKey}";

                // 3. Gọi Google Gemini
                var response = await _httpClient.PostAsync(url, content);
                var responseString = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode, $"Lỗi từ Google API: {responseString}");
                }

                // 4. Bóc tách Text trả về
                using var jsonDoc = JsonDocument.Parse(responseString);
                var root = jsonDoc.RootElement;
                
                // Trích xuất text từ response format của Gemini
                var botResponse = root.GetProperty("candidates")[0]
                                      .GetProperty("content")
                                      .GetProperty("parts")[0]
                                      .GetProperty("text").GetString();

                return Ok(new { answer = botResponse });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }
    }

    public class ChatBotRequestDto
    {
        public string Message { get; set; } = string.Empty;
        public List<ChatBotHistoryDto>? History { get; set; }
    }

    public class ChatBotHistoryDto
    {
        public string Role { get; set; } = string.Empty; // "user" hoặc "bot"
        public string Content { get; set; } = string.Empty;
    }
}
