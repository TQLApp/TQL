using Tql.Abstractions;

namespace Tql.Plugins.Jira.Categories;

internal static class TypeIds
{
    private static MatchTypeId CreateId(string id) => new(Guid.Parse(id), JiraPlugin.Id);

    public static readonly MatchTypeId Boards = CreateId("b984b4ae-0a4d-4b77-bc95-db675a6c96e9");
    public static readonly MatchTypeId Board = CreateId("c65a79b2-c42e-4fbe-a8f0-3eb37eba42e9");
    public static readonly MatchTypeId BoardQuickFilter = CreateId(
        "61b6a48e-175f-4bb2-9106-369f29ac3511"
    );

    public static readonly MatchTypeId Dashboards = CreateId(
        "c370eeb4-1481-4bb0-941a-de7ec1998480"
    );
    public static readonly MatchTypeId Dashboard = CreateId("5794d704-70e0-4bde-a7d2-1f6ca7e026d3");

    public static readonly MatchTypeId Issues = CreateId("4182a2c0-fffe-4280-a5e7-f5598272e616");
    public static readonly MatchTypeId Issue = CreateId("ae01c820-15d7-4f1a-99c8-93b0267a898b");

    public static readonly MatchTypeId News = CreateId("4c717644-7bd3-4b20-b3c9-3f5abda93604");
    public static readonly MatchTypeId New = CreateId("a44c93bb-3df8-4a21-9f0f-3063d28908fe");

    public static readonly MatchTypeId Projects = CreateId("2370f0ee-20d6-4d0c-8da0-2430449d395b");
    public static readonly MatchTypeId Project = CreateId("c8aa3d77-62fb-4dc8-bde6-d4ac58a2ad6b");
}
