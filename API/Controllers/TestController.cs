using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Mediator.Interfaces;
using Command.Chat;

namespace KnowledgeCollector.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TestController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("chat")]
        public async Task<IActionResult> SendMessage([FromBody] ChatRequest request)
        {
            await _mediator.Send(new BeginChat());
            return Ok();
        }
    }

    public record ChatRequest(string Message);
}
