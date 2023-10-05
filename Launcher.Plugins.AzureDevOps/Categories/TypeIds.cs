using Launcher.Abstractions;

namespace Launcher.Plugins.AzureDevOps.Categories;

internal static class TypeIds
{
    private static MatchTypeId CreateId(string id) => new(Guid.Parse(id), AzureDevOpsPlugin.Id);

    public static readonly MatchTypeId Backlogs = CreateId("14365335-45a2-43a3-af7d-fe2229744cc7");
    public static readonly MatchTypeId Backlog = CreateId("6d8e941f-1181-453f-b298-7c063e0a8da4");

    public static readonly MatchTypeId Boards = CreateId("8d85fc68-ac7c-4d25-a837-7ab475b073f6");
    public static readonly MatchTypeId Board = CreateId("e7836666-a0da-42f9-a793-9de3cad07099");

    public static readonly MatchTypeId Repositories = CreateId(
        "8d85fc68-ac7c-4d25-a837-7ab475b073f6"
    );
    public static readonly MatchTypeId Repository = CreateId(
        "e7836666-a0da-42f9-a793-9de3cad07099"
    );
    public static readonly MatchTypeId RepositoryFilePath = CreateId(
        "07461529-2c9a-4bda-be99-a94b8640fe7d"
    );
}
