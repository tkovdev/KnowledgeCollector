using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using KnowledgeCollector.Logic.Commands;

namespace KnowledgeCollector.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly OpenAIChat _openAIChat;
        public TestController(OpenAIChat openAIChat)
        {
            _openAIChat = openAIChat;
        }

        [HttpPost("chat")]
        public async Task<IActionResult> SendMessage([FromBody] ChatRequest request)
        {
            var response = await _openAIChat.SendMessageAsync(request.Message);
            return Ok(new { response });
        }
    }

    public record ChatRequest(string Message);
}
