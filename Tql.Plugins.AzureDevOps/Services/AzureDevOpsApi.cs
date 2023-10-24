using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.VisualStudio.Services.Client;
using Microsoft.VisualStudio.Services.Client.Controls;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.Common.TokenStorage;
using Microsoft.VisualStudio.Services.WebApi;
using Microsoft.Win32;
using NeoSmart.AsyncLock;
using Tql.Abstractions;

namespace Tql.Plugins.AzureDevOps.Services;

internal class AzureDevOpsApi
{
    private readonly IUI _ui;
    private readonly ILogger<AzureDevOpsApi> _logger;
    private readonly AsyncLock _lock = new();
    private readonly Dictionary<string, VssConnection> _connections = new();

    public AzureDevOpsApi(IUI ui, ILogger<AzureDevOpsApi> logger)
    {
        _ui = ui;
        _logger = logger;
    }

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
                    _logger.LogWarning(ex, "Connect failed, retrying");

                    // Clear any cached credentials.

                    using (
                        var key = Registry.CurrentUser.OpenSubKey(
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
                _ui.ShowNotificationBar(
                    $"{AzureDevOpsPlugin.Id}/ConnectionFailed/{collectionUri}",
                    $"Unable to connect to {collectionUri}. Click here to reconnect.",
                    () => RetryConnect(collectionUri)
                );
                throw;
            }

            return await _connections[collectionUri].GetClientAsync<T>();

            async Task<VssConnection> AttemptConnect()
            {
                var dialogHost = new DialogHost(_ui, $"Azure DevOps at {collectionUri}");

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
        using var key = Registry.CurrentUser.OpenSubKey(
            $@"Software\Microsoft\VSCommon\14.0\ClientServices\TokenStorage\Tql\{collectionUri}"
        );

        return key != null;
    }

    private class DialogHost : IDialogHost
    {
        private readonly IUI _ui;
        private readonly string _apiName;

        public VssConnection? Connection { get; set; }

        public DialogHost(IUI ui, string apiName)
        {
            _ui = ui;
            _apiName = apiName;
        }

        public async Task<bool?> InvokeDialogAsync(InvokeDialogFunc showDialog, object state)
        {
            var authentication = new InteractiveAuthentication(this, showDialog, state);

            await _ui.PerformInteractiveAuthentication(authentication);

            return authentication.Result;
        }

        private class InteractiveAuthentication : IInteractiveAuthentication
        {
            private readonly DialogHost _owner;
            private readonly InvokeDialogFunc _showDialog;
            private readonly object _state;

            public bool? Result { get; private set; }

            public string ResourceName => _owner._apiName;

            public InteractiveAuthentication(
                DialogHost owner,
                InvokeDialogFunc showDialog,
                object state
            )
            {
                _owner = owner;
                _showDialog = showDialog;
                _state = state;
            }

            public Task Authenticate(IWin32Window owner)
            {
                Result = _showDialog(owner.Handle, _state);

                return Task.CompletedTask;
            }
        }
    }
}
