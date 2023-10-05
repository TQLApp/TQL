using Launcher.Abstractions;
using Launcher.Utilities;

namespace Launcher.Plugins.AzureDevOps.Categories;

internal class NewType : IMatchType
{
    private readonly Images _images;

    public Guid Id => TypeIds.New.Id;

    public NewType(Images images)
    {
        _images = images;
    }

    public IMatch Deserialize(string json)
    {
        return new NewMatch(JsonSerializer.Deserialize<NewMatchDto>(json)!, _images);
    }
}
