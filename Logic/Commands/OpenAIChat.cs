using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using OpenAI.Chat;

namespace KnowledgeCollector.Logic.Commands;

public class OpenAIChat
{
    private readonly ChatClient _chatClient;
    public OpenAIChat(IConfiguration configuration)
    {
        var endpoint = configuration["AzureOpenAI:Endpoint"]
            ?? throw new InvalidOperationException("AzureOpenAI:Endpoint is not configured.");
        var deploymentName = configuration["AzureOpenAI:DeploymentName"]
            ?? throw new InvalidOperationException("AzureOpenAI:DeploymentName is not configured.");
        var tenantId = configuration["AzureAd:TenantId"]
            ?? throw new InvalidOperationException("AzureAd:TenantId is not configured.");
        var clientId = configuration["AzureAd:ClientId"]
            ?? throw new InvalidOperationException("AzureAd:ClientId is not configured.");
        var clientSecret = configuration["AzureAd:ClientSecret"]
            ?? throw new InvalidOperationException("AzureAd:ClientSecret is not configured.");

        var cred = new ClientSecretCredential(
            tenantId, clientId, clientSecret
        );
        var openAIClient = new AzureOpenAIClient(
            new Uri(endpoint),
            cred
        );
        _chatClient = openAIClient.GetChatClient(deploymentName);
    }

    public async Task<string> SendMessageAsync(string message)
    {
        ChatCompletion completion = await _chatClient.CompleteChatAsync(message);
        return completion.Content[0].Text;
    }
}