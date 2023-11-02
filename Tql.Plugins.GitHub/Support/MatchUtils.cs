using Tql.Plugins.GitHub.Categories;
using Tql.Utilities;

namespace Tql.Plugins.GitHub.Support;

internal static class MatchUtils
{
    public static string GetMatchLabel(string label, Configuration configuration, RootItemDto dto)
    {
        return GetMatchLabel(label, label, configuration, dto);
    }

    public static string GetMatchLabel(
        string globalLabel,
        string userLabel,
        Configuration configuration,
        RootItemDto dto
    )
    {
        var label = dto.Scope == RootItemScope.User ? userLabel : globalLabel;

        if (configuration.Connections.Length > 1)
        {
            var connection = configuration.Connections.Single(p => p.Id == dto.Id);

            return MatchText.ConnectionLabel(label, connection.Name);
        }

        return label;
    }
}
