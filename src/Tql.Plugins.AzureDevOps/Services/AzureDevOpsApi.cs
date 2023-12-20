using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using Tql.Abstractions;
using Tql.Utilities;

namespace Tql.Plugins.AzureDevOps.Services;

internal class AzureDevOpsApi(
    IEncryption encryption,
    ConfigurationManager configurationManager,
    IUI ui
)
{
    private readonly AsyncLock _lock = new();
    private readonly Dictionary<string, VssConnection> _connections = new();

    private async Task<VssConnection> GetConnection(string collectionUrl)
    {
        VssConnection? vssConnection;

        using (_lock)
        {
            if (!_connections.TryGetValue(collectionUrl, out vssConnection))
            {
                var connection = configurationManager.Configuration.Connections.Single(
                    p => p.Url == collectionUrl
                );

                vssConnection = new VssConnection(
                    new Uri(connection.Url.TrimEnd('/') + "/"),
                    new VssBasicCredential(
                        "",
                        encryption.DecryptString(connection.ProtectedPATToken)
                    )
                );

                try
                {
                    await EnsureCanConnect(vssConnection);
                }
                catch
                {
                    ui.ShowNotificationBar(
                        $"{AzureDevOpsPlugin.Id}/ConnectionFailed/{collectionUrl}",
                        string.Format(
                            Labels.AzureDevOpsApi_UnableToConnect,
                            string.Format(Labels.AzureDevOpsApi_ResourceName, collectionUrl),
                            Labels.AzureDevOpsPlugin_Title
                        ),
                        () => ui.OpenConfiguration(AzureDevOpsPlugin.ConfigurationPageId)
                    );
                    throw;
                }

                _connections.Add(collectionUrl, vssConnection);
            }
        }

        return vssConnection;
    }

    public async Task<T> GetClient<T>(string collectionUrl)
        where T : IVssHttpClient
    {
        var vssConnection = await GetConnection(collectionUrl);

        return await vssConnection.GetClientAsync<T>();
    }

    public async Task<Guid> GetUserId(string collectionUrl)
    {
        var vssConnection = await GetConnection(collectionUrl);

        return vssConnection.AuthorizedIdentity.Id;
    }

    private static async Task EnsureCanConnect(VssConnection vssConnection)
    {
        await vssConnection.ConnectAsync();

        // Force authentication.

        var client = await vssConnection.GetClientAsync<ProjectHttpClient>();

        await client.GetProjects();
    }
}
