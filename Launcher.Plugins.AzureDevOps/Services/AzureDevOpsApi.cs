﻿using System.Windows.Forms;
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
                var dialogHost = new DialogHost(_ui, $"Azure DevOps at {collectionUri}");

                var credentials = new VssClientCredentials(
                    new WindowsCredential(false),
                    new VssFederatedCredential(false),
                    dialogHost
                );

                credentials.Storage = new VssClientCredentialStorage(
                    collectionUri,
                    VssTokenStorageFactory.GetTokenStorageNamespace("Launcher")
                );

                connection = new VssConnection(new Uri(collectionUri), credentials);
                _connections.Add(collectionUri, connection);

                dialogHost.Connection = connection;

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
