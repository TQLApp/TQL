using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.VisualStudio.Services.Client;
using Microsoft.VisualStudio.Services.Client.Controls;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.Common.TokenStorage;
using Microsoft.VisualStudio.Services.WebApi;
using Microsoft.Win32;
using Tql.Abstractions;
using Tql.Utilities;

namespace Tql.Plugins.AzureDevOps.Services;

internal class AzureDevOpsApi(IUI ui, ILogger<AzureDevOpsApi> logger)
{
    private readonly AsyncLock _lock = new();
    private readonly Dictionary<string, VssConnection> _connections = new();

    public async Task<T> GetClient<T>(string collectionUri)
        where T : IVssHttpClient
    {
        using (await _lock.LockAsync())
        {
            try
            {
                try
                {
                    if (!_connections.ContainsKey(collectionUri))
                        _connections[collectionUri] = await AttemptConnect();
                }
                catch (Exception ex) when (CredentialsExist(collectionUri))
                {
                    logger.LogWarning(ex, "Connect failed, retrying");

                    // Clear any cached credentials.

                    using (
                        var key = Registry
                            .CurrentUser
                            .OpenSubKey(
                                @"Software\Microsoft\VSCommon\14.0\ClientServices\TokenStorage\Tql",
                                true
                            )
                    )
                    {
                        key?.DeleteSubKeyTree(collectionUri, false);
                    }

                    // Remove first because the connect attempt may still throw.
                    _connections.Remove(collectionUri);

                    _connections[collectionUri] = await AttemptConnect();
                }
            }
            catch
            {
                ui.ShowNotificationBar(
                    $"{AzureDevOpsPlugin.Id}/ConnectionFailed/{collectionUri}",
                    string.Format(
                        Labels.AzureDevOpsApi_UnableToConnect,
                        string.Format(Labels.AzureDevOpsApi_ResourceName, collectionUri)
                    ),
                    () => RetryConnect(collectionUri)
                );
                throw;
            }

            return await _connections[collectionUri].GetClientAsync<T>();

            async Task<VssConnection> AttemptConnect()
            {
                var dialogHost = new DialogHost(
                    ui,
                    string.Format(Labels.AzureDevOpsApi_ResourceName, collectionUri)
                );

                var credentials = new VssClientCredentials(
                    new WindowsCredential(false),
                    new VssFederatedCredential(false),
                    dialogHost
                );

                credentials.Storage = new VssClientCredentialStorage(
                    collectionUri,
                    VssTokenStorageFactory.GetTokenStorageNamespace("Tql")
                );

                var connection = new VssConnection(new Uri(collectionUri), credentials);

                dialogHost.Connection = connection;

                await connection.ConnectAsync();

                // Force authentication.

                var client = await connection.GetClientAsync<ProjectHttpClient>();

                await client.GetProjects();

                return connection;
            }
        }
    }

    private async void RetryConnect(string collectionUri)
    {
        try
        {
            await GetClient<ProjectHttpClient>(collectionUri);
        }
        catch
        {
            // Ignore.
        }
    }

    private static bool CredentialsExist(string collectionUri)
    {
        using var key = Registry
            .CurrentUser
            .OpenSubKey(
                $@"Software\Microsoft\VSCommon\14.0\ClientServices\TokenStorage\Tql\{collectionUri}"
            );

        return key != null;
    }

    private class DialogHost(IUI ui, string apiName) : IDialogHost
    {
        private readonly string _apiName = apiName;

        public VssConnection? Connection { get; set; }

        public async Task<bool?> InvokeDialogAsync(InvokeDialogFunc showDialog, object state)
        {
            var authentication = new InteractiveAuthentication(this, showDialog, state);

            await ui.PerformInteractiveAuthentication(authentication);

            return authentication.Result;
        }

        private class InteractiveAuthentication(
            DialogHost owner,
            InvokeDialogFunc showDialog,
            object state
        ) : IInteractiveAuthentication
        {
            public bool? Result { get; private set; }

            public string ResourceName => owner._apiName;

            public Task Authenticate(IWin32Window owner)
            {
                Result = showDialog(owner.Handle, state);

                return Task.CompletedTask;
            }
        }
    }
}
