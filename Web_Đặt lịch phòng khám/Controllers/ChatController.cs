using Microsoft.AspNetCore.Mvc;
using Web_Đặt_lịch_phòng_khám.Services;

namespace Web_Đặt_lịch_phòng_khám.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly IOpenAiService _openAiService;
        public ChatController(IOpenAiService openAiService)
        {
            _openAiService = openAiService;
        }

        [HttpPost("ask")]
        public async Task<IActionResult> Ask([FromBody] AskRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Question))
                return BadRequest(new { response = "Vui lòng nhập câu hỏi." });

            var answer = await _openAiService.GetChatResponse(request.Question);
            return Ok(new { response = answer });
        }
    }

    public class AskRequest
    {
        public string Question { get; set; }
    }
}