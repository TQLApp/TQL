using Launcher.Abstractions;
using Launcher.Utilities;

namespace Launcher.Plugins.AzureDevOps.Categories;

internal class BacklogType : IMatchType
{
    public static Guid Id = Guid.Parse("6d8e941f-1181-453f-b298-7c063e0a8da4");

    private readonly Images _images;

    Guid IMatchType.Id => Id;

    public BacklogType(Images images)
    {
        _images = images;
    }

    public IMatch Deserialize(string json)
    {
        return new BacklogMatch(JsonSerializer.Deserialize<BacklogMatchDto>(json)!, _images);
    }
}
