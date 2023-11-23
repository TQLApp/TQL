using Tql.Plugins.MicrosoftTeams.Services;
using Tql.Utilities;

namespace Tql.Plugins.MicrosoftTeams.Support;

internal static class MatchUtils
{
    public static string GetMatchLabel(
        string label,
        ConfigurationManager configurationManager,
        string id
    )
    {
        if (configurationManager.DirectoryIds.Length > 1)
        {
            var connection = configurationManager.GetDirectory(id)!;

            return MatchText.ConnectionLabel(label, connection.Name);
        }

        return label;
    }
}
