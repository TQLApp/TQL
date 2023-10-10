using Launcher.Abstractions;

namespace Launcher.Plugins.Azure.Categories;

internal static class TypeIds
{
    private static MatchTypeId CreateId(string id) => new(Guid.Parse(id), AzurePlugin.Id);

    public static readonly MatchTypeId Portals = CreateId("87999f55-daf6-41e7-a077-cb1d6f02d237");
    public static readonly MatchTypeId Portal = CreateId("585d59af-ef4f-44a0-8d0e-6863482714d8");
}
