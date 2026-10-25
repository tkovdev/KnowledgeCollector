using System;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using KnowledgeCollector.API.TokenService.Interfaces;

namespace KnowledgeCollector.API.TokenService
{
    public class AzureTokenService : ITokenService
    {
        private readonly TokenCredential _credential;
        private readonly string[] _scopes;

        public AzureTokenService(IConfiguration configuration, string[]? scopes = null)
        {
            var tenantId = configuration["AzureAd:TenantId"]
                ?? throw new InvalidOperationException("AzureAd:TenantId is not configured.");
            var clientId = configuration["AzureAd:ClientId"]
                ?? throw new InvalidOperationException("AzureAd:ClientId is not configured.");
            var clientSecret = configuration["AzureAd:ClientSecret"]
                ?? throw new InvalidOperationException("AzureAd:ClientSecret is not configured.");

            _scopes = scopes ?? new[] { "https://management.azure.com/.default" };
            _credential = new ClientSecretCredential(tenantId, clientId, clientSecret);
        }

        public async Task<string> GetTokenAsync(CancellationToken cancellationToken = default)
        {
            var context = new TokenRequestContext(_scopes);
            AccessToken token = await _credential.GetTokenAsync(context, cancellationToken);
            return token.Token;
        }
    }
}
