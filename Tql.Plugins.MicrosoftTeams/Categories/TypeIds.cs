using Tql.Abstractions;

namespace Tql.Plugins.MicrosoftTeams.Categories;

internal static class TypeIds
{
    private static MatchTypeId CreateId(string id) => new(Guid.Parse(id), MicrosoftTeamsPlugin.Id);

    public static readonly MatchTypeId Emails = CreateId("f1d9c46b-6297-4156-abed-09bf4f44d7cc");
    public static readonly MatchTypeId Email = CreateId("bee10761-ed35-4acf-8899-fc243869d10c");

    public static readonly MatchTypeId TeamsCalls = CreateId(
        "369e9586-29a5-4d1b-8df0-7d341050066d"
    );
    public static readonly MatchTypeId TeamsCall = CreateId("cc8d4d3a-2d37-4c61-87cf-17c9385c104d");

    public static readonly MatchTypeId TeamsChats = CreateId(
        "23abe171-b81d-4985-a78a-dc22c4e5ec3b"
    );
    public static readonly MatchTypeId TeamsChat = CreateId("d0dc927b-0240-4a33-a875-bec8ecd74139");

    public static readonly MatchTypeId TeamsVideos = CreateId(
        "7bd64918-24c9-4342-b893-eb9292e628de"
    );
    public static readonly MatchTypeId TeamsVideo = CreateId(
        "cf0b3b9f-8085-4be7-a8ae-cef9b6d03b0e"
    );
}
