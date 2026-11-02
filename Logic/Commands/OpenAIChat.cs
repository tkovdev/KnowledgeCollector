using Azure.AI.OpenAI;
using Azure.Identity;
using KnowledgeCollector.Logic.Services.TokenService.Interfaces;
using Microsoft.Extensions.Configuration;
using OpenAI.Chat;

namespace KnowledgeCollector.Logic.Commands;

public class OpenAIChat
{
    private readonly ChatClient _chatClient;
    public OpenAIChat(IConfiguration configuration, ITokenService tokenService)
    {
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

    public async Task<string> SendMessageAsync(string message)
    {
        var locationTool = ChatTool.CreateFunctionTool(
            functionName: nameof(GetCurrentLocation),
            functionDescription: "Get the user's current location"
        );
        List<ChatMessage> messages = [
            new UserChatMessage("Tell me an interesting fact about my location")
        ];
        ChatCompletionOptions opts = new()
        {
            Tools = { locationTool }
        };

        var reqAction = true;
        do
        {
            
            reqAction = false;
            ChatCompletion completion = _chatClient.CompleteChat(messages, opts);

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
                        switch (toolCall.FunctionName)
                        {
                            case nameof(GetCurrentLocation):
                                {
                                    var res = GetCurrentLocation();
                                    messages.Add(new ToolChatMessage(toolCall.Id, res));
                                    break;
                                }
                            default:
                                    {
                                        throw new NotImplementedException("Tool not defined");
                                    }
                        }
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
        return lastMessage.Content[0].Text.ToString() ?? throw new Exception("Messages with chat are empty");;
    }

    private static string GetCurrentLocation()
    {
        // Call the location API here.
        return "San Francisco";
    }
}