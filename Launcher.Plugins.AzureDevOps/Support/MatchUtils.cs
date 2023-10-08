using Launcher.Plugins.AzureDevOps.Services;

namespace Launcher.Plugins.AzureDevOps.Support;

internal static class MatchUtils
{
    public static string GetMatchLabel(
        string label,
        ConnectionManager connectionManager,
        string url
    )
    {
        if (connectionManager.Connections.Length > 1)
        {
            var connection = connectionManager.Connections.Single(p => p.Url == url);

            return $"{label} ({connection.Name})";
        }

        return label;
    }
}
