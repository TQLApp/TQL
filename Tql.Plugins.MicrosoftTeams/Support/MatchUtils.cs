using System.Net.Configuration;
using Tql.Abstractions;
using Tql.Plugins.MicrosoftTeams.Services;

namespace Tql.Plugins.MicrosoftTeams.Support;

internal static class MatchUtils
{
    public static string GetMatchLabel(
        string label,
        ConfigurationManager configurationManager,
        IPeopleDirectoryManager peopleDirectoryManager,
        string id
    )
    {
        if (configurationManager.DirectoryIds.Length > 1)
        {
            var connection = peopleDirectoryManager.Directories.Single(p => p.Id == id);

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
        if (!configurationManager.HasDirectory(id))
            return null;

        return peopleDirectoryManager.Directories.SingleOrDefault(p => p.Id == id);
    }
}
