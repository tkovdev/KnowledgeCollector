using Azure.AI.OpenAI;
using KnowledgeCollector.Intelligence.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using OpenAI.Chat;
using KnowledgeCollector.Intelligence.Tools;

namespace KnowledgeCollector.Intelligence.Commands;

public class OpenAIChatService : IAIChatService
{
    private readonly ChatClient _chatClient;
    private readonly Dictionary<string, ITool> _tools;
    public OpenAIChatService(IConfiguration configuration, ITokenService tokenService, IEnumerable<ITool> tools)
    {
        _tools = tools.ToDictionary(t => t.Name);
        var endpoint = configuration["AzureOpenAI:Endpoint"]
            ?? throw new InvalidOperationException("AzureOpenAI:Endpoint is not configured.");
        var deploymentName = configuration["AzureOpenAI:DeploymentName"]
            ?? throw new InvalidOperationException("AzureOpenAI:DeploymentName is not configured.");

        var cred = tokenService.GetTokenCredential();

        var openAIClient = new AzureOpenAIClient(
            new Uri(endpoint),
            cred
        );
        _chatClient = openAIClient.GetChatClient(deploymentName);
    }

    public async Task<List<ChatMessage>> SendMessageAsync(List<ChatMessage> messages)
    {
        ChatCompletionOptions opts = new();
        foreach (var t in _tools.Values)
            opts.Tools.Add(ChatTool.CreateFunctionTool(t.Name, t.Description, t.ParametersSchema));

        var reqAction = true;
        do
        {
            
            reqAction = false;
            ChatCompletion completion = await _chatClient.CompleteChatAsync(messages, opts);

            switch (completion.FinishReason)
            {
                case ChatFinishReason.Stop:
                {
                    messages.Add(new AssistantChatMessage(completion));
                    break;
                }

                case ChatFinishReason.ToolCalls:
                {
                    messages.Add(new AssistantChatMessage(completion));
                    foreach (var toolCall in completion.ToolCalls)
                    {
                        var tool = _tools.GetValueOrDefault(toolCall.FunctionName)
                            ?? throw new NotImplementedException($"Tool {toolCall.FunctionName} not defined");
                        var res = await tool.ExecuteAsync(toolCall.FunctionArguments.ToString());
                        messages.Add(new ToolChatMessage(toolCall.Id, res));
                    }
                    reqAction = true;
                    break;
                }

                case ChatFinishReason.Length:
                    throw new NotImplementedException("Incomplete model output due to MaxTokens parameter or token limit exceeded.");

                case ChatFinishReason.ContentFilter:
                    throw new NotImplementedException("Omitted content due to a content filter flag.");

                case ChatFinishReason.FunctionCall:
                    throw new NotImplementedException("Deprecated in favor of tool calls.");

                default:
                    throw new NotImplementedException(completion.FinishReason.ToString());
            }
        } while (reqAction);

        var lastMessage = messages.LastOrDefault() ?? throw new Exception("Messages with chat are empty");
        var lastMessageContent = lastMessage.Content[0].Text.ToString() ?? throw new Exception("Messages with chat are empty");

        return messages;
    }
}