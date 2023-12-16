using System.Windows.Forms;
using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using Tql.Abstractions;
using Tql.Utilities;

namespace Tql.Plugins.Azure.Services;

internal class AzureApi(IUI ui, ConfigurationManager configurationManager)
{
    private readonly AsyncLock _lock = new();
    private readonly Dictionary<Guid, ArmClient> _clients = new();

    public async Task<ArmClient> GetClient(Guid id)
    {
        using (await _lock.LockAsync())
        {
            if (!_clients.TryGetValue(id, out var client))
            {
                var connection = configurationManager.Configuration.Connections.Single(
                    p => p.Id == id
                );

                try
                {
                    client = await CreateClient(connection);
                }
                catch
                {
                    ui.ShowNotificationBar(
                        $"{AzurePlugin.Id}/ConnectionFailed/{id}",
                        string.Format(
                            Labels.AzureApi_UnableToConnect,
                            string.Format(Labels.AzureApi_ResourceName, connection.Name)
                        ),
                        () => RetryConnect(id)
                    );
                    throw;
                }

                _clients[id] = client;
            }

            return client;
        }
    }

    private async void RetryConnect(Guid id)
    {
        try
        {
            await GetClient(id);
        }
        catch
        {
            // Ignore.
        }
    }

    private async Task<ArmClient> CreateClient(Connection connection)
    {
        var tokenCachePersistenceOptions = new TokenCachePersistenceOptions
        {
            Name = $"Tql_{connection.Id}"
        };

        var credential = new ChainedTokenCredential(
            new SharedTokenCacheCredential(
                new SharedTokenCacheCredentialOptions(tokenCachePersistenceOptions)
                {
                    TenantId = connection.TenantId
                }
            ),
            new UICredential(
                new InteractiveBrowserCredential(
                    new InteractiveBrowserCredentialOptions
                    {
                        TokenCachePersistenceOptions = tokenCachePersistenceOptions,
                        TenantId = connection.TenantId
                    }
                ),
                ui,
                string.Format(Labels.AzureApi_ResourceName, connection.Name)
            )
        );

        var client = new ArmClient(credential);

        // Force authentication.
        await client.GetDefaultSubscriptionAsync();

        return client;
    }

    private class UICredential(TokenCredential item, IUI ui, string resourceName) : TokenCredential
    {
        public override async ValueTask<AccessToken> GetTokenAsync(
            TokenRequestContext requestContext,
            CancellationToken cancellationToken
        )
        {
            var accessToken = default(AccessToken);

            await ui.PerformInteractiveAuthentication(
                new InteractiveAuthentication(
                    resourceName,
                    async () =>
                        accessToken = await item.GetTokenAsync(requestContext, cancellationToken)
                )
            );

            return accessToken;
        }

        public override AccessToken GetToken(
            TokenRequestContext requestContext,
            CancellationToken cancellationToken
        )
        {
            throw new NotSupportedException();
        }
    }

    private class InteractiveAuthentication(string resourceName, Func<Task> action)
        : IInteractiveAuthentication
    {
        public string ResourceName { get; } = resourceName;

        public async Task Authenticate(IWin32Window owner)
        {
            await action();
        }
    }
}
