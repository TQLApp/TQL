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
}
