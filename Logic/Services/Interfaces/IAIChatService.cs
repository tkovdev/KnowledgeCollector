namespace KnowledgeCollector.Logic.Services.Interfaces;

/// <summary>
/// Exposes functions to execute queries against the AI infrastructure
/// </summary>
public interface IAIChatService
{
    public Task<string> SendMessageAsync(string instruction, string message);
    
}