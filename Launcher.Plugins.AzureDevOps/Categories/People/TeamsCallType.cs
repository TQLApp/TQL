using Launcher.Abstractions;
using Launcher.Utilities;

namespace Launcher.Plugins.AzureDevOps.Categories.People;

internal class TeamsCallType : IMatchType
{
    private readonly Images _images;

    public Guid Id => TypeIds.TeamsCall.Id;

    public TeamsCallType(Images images)
    {
        _images = images;
    }

    public IMatch Deserialize(string json)
    {
        return new TeamsCallMatch(JsonSerializer.Deserialize<GraphUserDto>(json)!, _images);
    }
}
