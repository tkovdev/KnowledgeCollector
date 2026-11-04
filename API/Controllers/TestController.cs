using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using KnowledgeCollector.Logic.Commands;
using KnowledgeCollector.Logic.Services.Interfaces;

namespace KnowledgeCollector.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly IAIChatService _aiChat;
        public TestController(IAIChatService aiChat)
        {
            _aiChat = aiChat;
        }

        [HttpPost("chat")]
        public async Task<IActionResult> SendMessage([FromBody] ChatRequest request)
        {
            var instruction = """
                You are tasked with creating one valuable question to ask the user regarding their team, to uncover processes and elicit information that will later be used to generate a quiz for others to assess their knowledge on the user's team.
            """;
            var response = await _aiChat.SendMessageAsync(instruction, request.Message);
            return Ok(new { response });
        }
    }

    public record ChatRequest(string Message);
}
