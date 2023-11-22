using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using Tql.Abstractions;

namespace Tql.Plugins.AzureDevOps.Services;

internal class AzureDevOpsApi(IEncryption encryption, ConfigurationManager configurationManager)
{
    private readonly object _syncRoot = new();
    private readonly Dictionary<string, VssConnection> _connections = new();

    public async Task<T> GetClient<T>(string collectionUrl)
        where T : IVssHttpClient
    {
        VssConnection? vssConnection;

        lock (_syncRoot)
        {
            if (!_connections.TryGetValue(collectionUrl, out vssConnection))
            {
                var connection = configurationManager
                    .Configuration
                    .Connections
                    .Single(p => p.Url == collectionUrl);

                vssConnection = new VssConnection(
                    new Uri(connection.Url.TrimEnd('/') + "/"),
                    new VssBasicCredential(
                        "",
                        encryption.DecryptString(connection.ProtectedPATToken)
                    )
                );
                _connections.Add(collectionUrl, vssConnection);
            }
        }

        return await vssConnection.GetClientAsync<T>();
    }
}
