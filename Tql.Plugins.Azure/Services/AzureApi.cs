using System.Windows.Forms;
using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using Microsoft.Extensions.Logging;
using NeoSmart.AsyncLock;
using Tql.Abstractions;

namespace Tql.Plugins.Azure.Services;

internal class AzureApi
{
    private readonly ILogger<AzureApi> _logger;
    private readonly IUI _ui;
    private readonly ConfigurationManager _configurationManager;
    private readonly AsyncLock _lock = new();
    private readonly Dictionary<Guid, ArmClient> _clients = new();

    public AzureApi(ILogger<AzureApi> logger, IUI ui, ConfigurationManager configurationManager)
    {
        _logger = logger;
        _ui = ui;
        _configurationManager = configurationManager;
    }

    public async Task<ArmClient> GetClient(Guid id)
    {
        using (await _lock.LockAsync())
        {
            if (!_clients.TryGetValue(id, out var client))
            {
                var connection = _configurationManager.Configuration.Connections.Single(
                    p => p.Id == id
                );

                try
                {
                    client = await CreateClient(connection);
                }
                catch
                {
                    _ui.ShowNotificationBar(
                        $"{AzurePlugin.Id}/ConnectionFailed/{id}",
                        $"Unable to connect to Azure - {connection.Name}. Click here to reconnect.",
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
            ),
            new UICredential(
                new InteractiveBrowserCredential(
                    new InteractiveBrowserCredentialOptions
                    {
                        TokenCachePersistenceOptions = tokenCachePersistenceOptions,
                    }
                ),
                _ui,
                $"Azure - {connection.Name}"
            )
        );

        var client = new ArmClient(credential);

        // Force authentication.
        await client.GetDefaultSubscriptionAsync();

        return client;
    }

    private class UICredential : TokenCredential
    {
        private readonly TokenCredential _item;
        private readonly IUI _ui;
        private readonly string _resourceName;

        public UICredential(TokenCredential item, IUI ui, string resourceName)
        {
            _item = item;
            _ui = ui;
            _resourceName = resourceName;
        }

        public override async ValueTask<AccessToken> GetTokenAsync(
            TokenRequestContext requestContext,
            CancellationToken cancellationToken
        )
        {
            var accessToken = default(AccessToken);

            await _ui.PerformInteractiveAuthentication(
                new InteractiveAuthentication(
                    _resourceName,
                    async () =>
                        accessToken = await _item.GetTokenAsync(requestContext, cancellationToken)
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

    private class InteractiveAuthentication : IInteractiveAuthentication
    {
        private readonly Func<Task> _action;

        public string ResourceName { get; }

        public InteractiveAuthentication(string resourceName, Func<Task> action)
        {
            _action = action;
            ResourceName = resourceName;
        }

        public async Task Authenticate(IWin32Window owner)
        {
            await _action();
        }
    }
}
