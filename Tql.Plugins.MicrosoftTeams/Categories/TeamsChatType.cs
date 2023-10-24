using Tql.Abstractions;
using Tql.Utilities;

namespace Tql.Plugins.MicrosoftTeams.Categories;

internal class TeamsChatType : IMatchType
{
    public Guid Id => TypeIds.TeamsChat.Id;

    public IMatch Deserialize(string json)
    {
        return new TeamsChatMatch(JsonSerializer.Deserialize<PersonDto>(json)!);
    }
}
