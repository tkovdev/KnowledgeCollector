using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using KnowledgeCollector.Intelligence.Services.Interfaces;
using System.Security.Cryptography.X509Certificates;

namespace KnowledgeCollector.Intelligence.Services
{
    public class AzureTokenService : ITokenService
    {
        private readonly X509Certificate2 _certificate;
        private readonly TokenCredential _credential;
        private readonly string[] _scopes;

        public AzureTokenService(IConfiguration configuration, string[]? scopes = null)
        {
            var tenantId = configuration["AzureAd:TenantId"]
                ?? throw new InvalidOperationException("AzureAd:TenantId is not configured.");
            var clientId = configuration["AzureAd:ClientId"]
                ?? throw new InvalidOperationException("AzureAd:ClientId is not configured.");
            var certRelativePath = configuration["AzureAd:CertificateRelativePath"]
                ?? throw new InvalidOperationException("AzureAd:ClientSecret is not configured.");

            //KeyVault can be used to generate the PFX, create the PFX and export it in Pkcs12. Passwords will be blank. Certificate can be included in the /Certificates path of the build. Do not commit PFX to code.
            string certPath = Path.Combine(AppContext.BaseDirectory, certRelativePath);

            if (!File.Exists(certPath))
            {
                throw new FileNotFoundException($"The certificate file could not be found at: {certPath}");
            }

            try
            {
                _certificate = X509CertificateLoader.LoadPkcs12FromFile(certPath, String.Empty);
            } catch
            {
                if (_certificate is not null) _certificate.Dispose();
                throw;
            }

            _scopes = scopes ?? new[] { "https://management.azure.com/.default" };
            _credential = new ClientCertificateCredential(tenantId, clientId, _certificate);
        }

        public TokenCredential GetTokenCredential()
        {
            return _credential;
        }
    }
}
