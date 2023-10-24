namespace Tql.Plugins.Confluence.Support;

internal static class MatchUtils
{
    public static string GetMatchLabel(string label, Configuration configuration, string url)
    {
        if (configuration.Connections.Length > 1)
        {
            var connection = configuration.Connections.Single(p => p.Url == url);

            return $"{label} ({connection.Name})";
        }

        return label;
    }
}
