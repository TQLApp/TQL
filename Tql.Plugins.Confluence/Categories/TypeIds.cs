using Tql.Abstractions;

namespace Tql.Plugins.Confluence.Categories;

internal static class TypeIds
{
    private static MatchTypeId CreateId(string id) => new(Guid.Parse(id), ConfluencePlugin.Id);

    public static readonly MatchTypeId Spaces = CreateId("15dbd194-0949-44f5-bddb-9a09f154e7f5");
    public static readonly MatchTypeId Space = CreateId("d920e665-9a8f-4142-bdbb-b97a9c8b12a8");
}
