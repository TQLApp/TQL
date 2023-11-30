using Tql.Abstractions;

namespace Tql.Plugins.AzureDevOps.Categories;

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

    public static readonly MatchTypeId Pipelines = CreateId("ad43b94d-b94d-4974-8bd9-9d479ed9c9c7");
    public static readonly MatchTypeId Pipeline = CreateId("34e37fdc-6843-4dd4-8cb8-5d9d299a4aa7");

    public static readonly MatchTypeId Queries = CreateId("d2fc58ee-11a2-4f3e-86d5-82cac7d8a502");
    public static readonly MatchTypeId Query = CreateId("e3f61eb6-3a0d-4f81-aacd-a730952be408");

    public static readonly MatchTypeId Repositories = CreateId(
        "b2b479f8-572e-4a02-a87e-c1ba244b17cf"
    );
    public static readonly MatchTypeId Repository = CreateId(
        "c43afa23-6d71-4940-ba4c-256d1a5d13bb"
    );
    public static readonly MatchTypeId RepositoryFilePath = CreateId(
        "07461529-2c9a-4bda-be99-a94b8640fe7d"
    );

    public static readonly MatchTypeId WorkItems = CreateId("3d57f05c-fbdd-4383-b305-3f48b2f018d2");
    public static readonly MatchTypeId WorkItem = CreateId("f73482ba-e069-4897-899a-fad1afbdb5f5");
}
