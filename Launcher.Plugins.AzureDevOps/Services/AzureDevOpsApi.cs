using Launcher.Abstractions;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.VisualStudio.Services.Client;
using Microsoft.VisualStudio.Services.Client.Controls;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.Common.TokenStorage;
using Microsoft.VisualStudio.Services.WebApi;
using NeoSmart.AsyncLock;

namespace Launcher.Plugins.AzureDevOps.Services;

internal class AzureDevOpsApi : IAzureDevOpsApi
{
    private readonly IUI _ui;
    private readonly AsyncLock _lock = new();
    private readonly Dictionary<string, VssConnection> _connections = new();

    public AzureDevOpsApi(IUI ui)
    {
        _ui = ui;
    }

    public async Task<T> GetClient<T>(string collectionUri)
        where T : IVssHttpClient
    {
        using (await _lock.LockAsync())
        {
            if (!_connections.TryGetValue(collectionUri, out var connection))
            {
                var credentials = new VssClientCredentials(
                    new WindowsCredential(false),
                    new VssFederatedCredential(false),
                    new DialogHost(_ui)
                );

                credentials.Storage = new VssClientCredentialStorage(
                    collectionUri,
                    VssTokenStorageFactory.GetTokenStorageNamespace("Launcher")
                );

                connection = new VssConnection(new Uri(collectionUri), credentials);
                _connections.Add(collectionUri, connection);

                await connection.ConnectAsync();

                // Force authentication.

                var client = await connection.GetClientAsync<ProjectHttpClient>();
                await client.GetProjects();
            }

            return await connection.GetClientAsync<T>();
        }
    }

    private class DialogHost : IDialogHost
    {
        private readonly IUI _ui;

        public DialogHost(IUI ui) => _ui = ui;

        public Task<bool?> InvokeDialogAsync(InvokeDialogFunc showDialog, object state) =>
            _ui.RunOnAuthenticationThread(owner => showDialog(owner.Handle, state));
    }
}
