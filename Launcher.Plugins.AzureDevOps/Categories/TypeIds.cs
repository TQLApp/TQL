using Launcher.Abstractions;

namespace Launcher.Plugins.AzureDevOps.Categories;

internal static class TypeIds
{
    private static MatchTypeId CreateId(string id) => new(Guid.Parse(id), AzureDevOpsPlugin.Id);

    public static readonly MatchTypeId Backlogs = CreateId("14365335-45a2-43a3-af7d-fe2229744cc7");
    public static readonly MatchTypeId Backlog = CreateId("6d8e941f-1181-453f-b298-7c063e0a8da4");

    public static readonly MatchTypeId Boards = CreateId("8d85fc68-ac7c-4d25-a837-7ab475b073f6");
    public static readonly MatchTypeId Board = CreateId("e7836666-a0da-42f9-a793-9de3cad07099");

    public static readonly MatchTypeId Dashboards = CreateId(
        "bfa272e9-988c-4ede-b33a-4e926e78a1c3"
    );
    public static readonly MatchTypeId Dashboard = CreateId("c1779024-b674-4244-9a97-771a9c18ffc4");

    public static readonly MatchTypeId News = CreateId("2e9aece7-2741-4559-8b81-3964be627c81");
    public static readonly MatchTypeId New = CreateId("1149d624-9dcf-4fc5-bdf5-41f2bdfd2217");

    public static readonly MatchTypeId Repositories = CreateId(
        "b2b479f8-572e-4a02-a87e-c1ba244b17cf"
    );
    public static readonly MatchTypeId Repository = CreateId(
        "c43afa23-6d71-4940-ba4c-256d1a5d13bb"
    );
    public static readonly MatchTypeId RepositoryFilePath = CreateId(
        "07461529-2c9a-4bda-be99-a94b8640fe7d"
    );
}
