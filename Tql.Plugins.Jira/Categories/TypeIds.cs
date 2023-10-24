using Tql.Abstractions;

namespace Tql.Plugins.Jira.Categories;

internal static class TypeIds
{
    private static MatchTypeId CreateId(string id) => new(Guid.Parse(id), JiraPlugin.Id);

    public static readonly MatchTypeId Dashboards = CreateId(
        "c370eeb4-1481-4bb0-941a-de7ec1998480"
    );
    public static readonly MatchTypeId Dashboard = CreateId("5794d704-70e0-4bde-a7d2-1f6ca7e026d3");
}
