
using OpenAI.Chat;

namespace KnowledgeCollector.Intelligence.Services.Interfaces;

/// <summary>
/// Exposes functions to execute queries against the AI infrastructure
/// </summary>
public interface IAIChatService
{
    public Task<List<ChatMessage>> SendMessageAsync(List<ChatMessage> messages);
    
}