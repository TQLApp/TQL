using Launcher.Abstractions;
using Launcher.Utilities;

namespace Launcher.Plugins.AzureDevOps.Categories.People;

internal class TeamsChatType : IMatchType
{
    private readonly Images _images;

    public Guid Id => TypeIds.TeamsChat.Id;

    public TeamsChatType(Images images)
    {
        _images = images;
    }

    public IMatch Deserialize(string json)
    {
        return new TeamsChatMatch(JsonSerializer.Deserialize<GraphUserDto>(json)!, _images);
    }
}
