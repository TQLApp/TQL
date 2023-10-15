using Tql.Abstractions;
using Tql.Utilities;

namespace Tql.Plugins.AzureDevOps.Categories.People;

internal class TeamsChatType : IMatchType
{
    public Guid Id => TypeIds.TeamsChat.Id;

    public IMatch Deserialize(string json)
    {
        return new TeamsChatMatch(JsonSerializer.Deserialize<GraphUserDto>(json)!);
    }
}
