using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace Authentication;

internal sealed class JwksConfigurationRetriever :
    IConfigurationRetriever<OpenIdConnectConfiguration>
{
    public async Task<OpenIdConnectConfiguration> GetConfigurationAsync(
        string address,
        IDocumentRetriever retriever,
        CancellationToken cancel)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(address);
        ArgumentNullException.ThrowIfNull(retriever);

        var document = await retriever.GetDocumentAsync(address, cancel);
        var jsonWebKeySet = new JsonWebKeySet(document);
        var configuration = new OpenIdConnectConfiguration { JwksUri = address };

        foreach (var signingKey in jsonWebKeySet.GetSigningKeys())
        {
            configuration.SigningKeys.Add(signingKey);
        }

        if (configuration.SigningKeys.Count == 0)
        {
            throw new SecurityTokenException(
                $"The JWKS endpoint '{address}' returned no signing keys.");
        }

        return configuration;
    }
}
