using Launcher.Abstractions;
using Launcher.Utilities;

namespace Launcher.Plugins.AzureDevOps.Categories.People;

internal class TeamsVideoType : IMatchType
{
    public Guid Id => TypeIds.TeamsVideo.Id;

    public IMatch Deserialize(string json)
    {
        return new TeamsVideoMatch(JsonSerializer.Deserialize<GraphUserDto>(json)!);
    }
}
