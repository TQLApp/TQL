using Tql.Abstractions;

namespace Tql.Plugins.Jira.Categories;

internal static class TypeIds
{
    private static MatchTypeId CreateId(string id) => new(Guid.Parse(id), JiraPlugin.Id);

    public static readonly MatchTypeId Dashboards = CreateId(
        "c370eeb4-1481-4bb0-941a-de7ec1998480"
    );
    public static readonly MatchTypeId Dashboard = CreateId("5794d704-70e0-4bde-a7d2-1f6ca7e026d3");

    public static readonly MatchTypeId Issues = CreateId("4182a2c0-fffe-4280-a5e7-f5598272e616");
    public static readonly MatchTypeId Issue = CreateId("ae01c820-15d7-4f1a-99c8-93b0267a898b");

    public static readonly MatchTypeId Projects = CreateId("2370f0ee-20d6-4d0c-8da0-2430449d395b");
    public static readonly MatchTypeId Project = CreateId("c8aa3d77-62fb-4dc8-bde6-d4ac58a2ad6b");
}
