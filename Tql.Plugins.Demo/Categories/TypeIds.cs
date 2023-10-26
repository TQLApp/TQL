using Tql.Abstractions;

namespace Tql.Plugins.Demo.Categories;

internal static class TypeIds
{
    private static MatchTypeId CreateId(string id) => new(Guid.Parse(id), DemoPlugin.Id);

    public static readonly MatchTypeId Demoes = CreateId("8c6215b5-3698-4ef9-8ac2-13bd0df59b88");
    public static readonly MatchTypeId Demo = CreateId("779e906f-c409-48ee-9df4-769083d3ff64");
}
