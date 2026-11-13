using Azure.Core;

namespace KnowledgeCollector.Intelligence.Services.Interfaces;

/// <summary>
/// Acquires access tokens from Azure (Microsoft Entra ID) for the service principal.
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Returns instantiated token credential for Azure
    /// </summary>
    public TokenCredential GetTokenCredential();
}
