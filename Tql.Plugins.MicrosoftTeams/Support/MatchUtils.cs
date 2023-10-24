using Tql.Abstractions;
using Tql.Plugins.MicrosoftTeams.Services;

namespace Tql.Plugins.MicrosoftTeams.Support;

internal static class MatchUtils
{
    public static string GetMatchLabel(
        string label,
        IPeopleDirectoryManager peopleDirectoryManager,
        string id
    )
    {
        var directories = peopleDirectoryManager.Directories;
        if (directories.Length > 1)
        {
            var connection = directories.Single(p => p.Id == id);

            return $"{label} ({connection.Name})";
        }

        return label;
    }

    public static IPeopleDirectory? GetDirectory(
        ConfigurationManager configurationManager,
        IPeopleDirectoryManager peopleDirectoryManager,
        string id
    )
    {
        var configuration = configurationManager.Configuration;

        if (
            configuration.Mode == ConfigurationMode.Selected
            && !configuration.DirectoryIds.Contains(id)
        )
            return null;

        return peopleDirectoryManager.Directories.SingleOrDefault(p => p.Id == id);
    }
}
