using Launcher.Abstractions;
using Launcher.Utilities;

namespace Launcher.Plugins.AzureDevOps.Categories.People;

internal class TeamsVideoType : IMatchType
{
    private readonly Images _images;

    public Guid Id => TypeIds.TeamsVideo.Id;

    public TeamsVideoType(Images images)
    {
        _images = images;
    }

    public IMatch Deserialize(string json)
    {
        return new TeamsVideoMatch(JsonSerializer.Deserialize<GraphUserDto>(json)!, _images);
    }
}
