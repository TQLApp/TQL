using System.Collections.Specialized;
using Microsoft.Extensions.Logging;
using OAuth2.Configuration;
using OAuth2.Infrastructure;
using Octokit;
using Tql.Abstractions;
using GitHubClient = OAuth2.Client.Impl.GitHubClient;

namespace Tql.Plugins.GitHub.Services;

internal class GitHubOAuthWorkflow(
    string clientId,
    string clientSecret,
    string scope,
    string redirectUri,
    IUI ui,
    ILogger logger
)
{
    public async Task<Credentials> Authorize()
    {
        var client = new GitHubClient(
            new RequestFactory(),
            new ClientConfiguration
            {
                ClientId = clientId,
                ClientSecret = clientSecret,
                Scope = scope,
                RedirectUri = redirectUri
            }
        );

        logger.LogInformation("Redirect URI {RedirectURI}", redirectUri);

        NameValueCollection queryString;

        using (var httpServer = new SimpleHttpServer(redirectUri, logger))
        {
            var authorizationRequest = await client.GetLoginLinkUriAsync();

            ui.OpenUrl(authorizationRequest);

            queryString = await httpServer.GetResponse(Constants.AuthenticationTimeout);
        }

        var accessToken = await client.GetTokenAsync(queryString);

        return new Credentials(accessToken, AuthenticationType.Bearer);
    }
}
