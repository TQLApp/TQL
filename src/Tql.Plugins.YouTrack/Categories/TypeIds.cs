using Tql.Abstractions;

namespace Tql.Plugins.YouTrack.Categories;

internal static class TypeIds
{
    private static MatchTypeId CreateId(string id) => new(Guid.Parse(id), YouTrackPlugin.Id);

    public static readonly MatchTypeId Boards = CreateId("836195e8-2fdd-4280-9079-a9541409820a");
    public static readonly MatchTypeId Board = CreateId("05d3125b-4e61-4e65-a8b9-b79ab1f2924a");

    public static readonly MatchTypeId Dashboards = CreateId(
        "ec125bd1-8f26-4ead-90ad-285a31e94719"
    );
    public static readonly MatchTypeId Dashboard = CreateId("ec8ffed0-4b1f-4da7-92c2-d26c533ae54e");

    public static readonly MatchTypeId Issues = CreateId("f0a983c9-e93d-4995-bb36-e4857167cfc1");
    public static readonly MatchTypeId Issue = CreateId("eec71d7f-237f-4120-9dd5-122f53e551fd");

    public static readonly MatchTypeId News = CreateId("c4780f73-fe57-4b6d-b911-7e0aa1f914a2");
    public static readonly MatchTypeId New = CreateId("4c780a20-d582-40e9-9668-616e07491495");

    public static readonly MatchTypeId Projects = CreateId("0268f2d0-7c99-46c2-a6f4-7a1379ea7c9e");
    public static readonly MatchTypeId Project = CreateId("dafbacd3-ff99-4de2-bd7f-baa871399aa1");

    public static readonly MatchTypeId SavedSearches = CreateId(
        "c0b268e5-a45c-441a-ab89-42c5171857df"
    );
    public static readonly MatchTypeId SavedSearch = CreateId(
        "cc06857b-d24a-4d9c-916a-c1e95801fc2e"
    );
}
