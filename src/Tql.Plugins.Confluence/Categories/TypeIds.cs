using Tql.Abstractions;

namespace Tql.Plugins.Confluence.Categories;

internal static class TypeIds
{
    private static MatchTypeId CreateId(string id) => new(Guid.Parse(id), ConfluencePlugin.Id);

    public static readonly MatchTypeId Searches = CreateId("89f7fb76-377b-4609-a2a1-d064a18c06c4");
    public static readonly MatchTypeId Search = CreateId("4fcf42d5-36fa-411d-9c16-b71363110ceb");

    public static readonly MatchTypeId Spaces = CreateId("15dbd194-0949-44f5-bddb-9a09f154e7f5");
    public static readonly MatchTypeId Space = CreateId("d920e665-9a8f-4142-bdbb-b97a9c8b12a8");
}
