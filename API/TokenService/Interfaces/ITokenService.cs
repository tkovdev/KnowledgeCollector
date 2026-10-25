namespace KnowledgeCollector.API.TokenService.Interfaces;

/// <summary>
/// Acquires access tokens from Azure (Microsoft Entra ID) for the service principal.
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Acquires an access token for the given scopes using the service principal credentials.
    /// </summary>
    Task<string> GetTokenAsync(CancellationToken cancellationToken = default);
}
