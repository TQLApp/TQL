using Launcher.Plugins.Azure.Services;

namespace Launcher.Plugins.Azure.Support;

internal static class MatchUtils
{
    public static string GetMatchLabel(string label, ConnectionManager connectionManager, Guid id)
    {
        if (connectionManager.Connections.Length > 1)
        {
            var connection = connectionManager.Connections.Single(p => p.Id == id);

            return $"{label} ({connection.Name})";
        }

        return label;
    }
}
