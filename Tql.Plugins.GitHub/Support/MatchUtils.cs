using Tql.Plugins.GitHub.Categories;
using Tql.Plugins.GitHub.Services;

namespace Tql.Plugins.GitHub.Support;

internal static class MatchUtils
{
    public static string GetMatchLabel(
        string label,
        ConnectionManager connectionManager,
        RootItemDto dto
    )
    {
        if (connectionManager.Connections.Length > 1)
        {
            var connection = connectionManager.Connections.Single(p => p.Id == dto.Id);

            label = $"{label} ({connection.Name})";
        }

        if (dto.Scope == RootItemScope.User)
            label = $"My {label}";

        return label;
    }
}
