using Launcher.Abstractions;

namespace Launcher.Plugins.GitHub.Categories;

internal static class TypeIds
{
    private static MatchTypeId CreateId(string id) => new(Guid.Parse(id), GitHubPlugin.Id);

    public static readonly MatchTypeId Repositories = CreateId(
        "d4d42f26-f777-46c5-895b-b18287fd6fb9"
    );
    public static readonly MatchTypeId Repository = CreateId(
        "2214bb85-7004-49c1-a8e3-61b0c8a68e4b"
    );

    public static readonly MatchTypeId Issues = CreateId("7ff14022-2f3d-486e-9ccd-77051c47e3db");
    public static readonly MatchTypeId Issue = CreateId("b1f1dca8-8973-4cac-9cd5-d0e5b27b334a");

    public static readonly MatchTypeId PullRequests = CreateId(
        "b2e9f741-9336-44ab-b740-b386e23ad702"
    );
    public static readonly MatchTypeId PullRequest = CreateId(
        "211d36ac-c317-42d8-a6c7-d14f65e4ad91"
    );

    public static readonly MatchTypeId Users = CreateId("f6714e76-8784-429e-a67d-e332e7b55b97");
    public static readonly MatchTypeId User = CreateId("10f03c08-66f5-49c1-909f-277925a5888e");
}
