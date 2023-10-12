using Launcher.Abstractions;
using Launcher.Utilities;

namespace Launcher.Plugins.AzureDevOps.Categories.People;

internal class TeamsChatType : IMatchType
{
    public Guid Id => TypeIds.TeamsChat.Id;

    public IMatch Deserialize(string json)
    {
        return new TeamsChatMatch(JsonSerializer.Deserialize<GraphUserDto>(json)!);
    }
}
