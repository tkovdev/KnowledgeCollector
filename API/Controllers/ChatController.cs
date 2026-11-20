using Command.Chat;
using Mediator.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MyApp.Namespace
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ChatController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> BeginChat([FromBody] ChatRequest request)
        {
            var sessionId = await _mediator.Send(new BeginChat());
            return Ok(new { SessionId = sessionId.ToString() });
        }

        [HttpPatch]
        public async Task<IActionResult> ContinueChat([FromBody] ChatRequest request)
        {
            var sessionId = await _mediator.Send(new ContinueChat(request.SessionId ?? throw new ArgumentNullException("SessionId cannot be null"),
                                                                  request.Message
                                                                  )
                                                );
            return Ok(new { SessionId = sessionId.ToString() });
        }
    }
}
    public record ChatRequest(string? SessionId, string Message);

