using Command.Chat;
using Data.Mappers;
using Mediator.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using Query.Chat;

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

        [HttpGet("{sessionId}")]
        public async Task<IActionResult> GetChat([FromRoute] string sessionId)
        {
            var id = ObjectId.Parse(sessionId);
            var history = await _mediator.Send(new GetChatSession(){Id = id});
            if (history is null)
                return NotFound();

            return Ok(SessionMapper.ToJson(history));
        }        
        
        [HttpPost]
        public async Task<IActionResult> BeginChat()
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
