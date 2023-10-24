namespace Tql.Plugins.Azure.Support;

internal static class MatchUtils
{
    public static string GetMatchLabel(string label, Configuration configuration, Guid id)
    {
        if (configuration.Connections.Length > 1)
        {
            var connection = configuration.Connections.Single(p => p.Id == id);

            return $"{label} ({connection.Name})";
        }

        return label;
    }
}
