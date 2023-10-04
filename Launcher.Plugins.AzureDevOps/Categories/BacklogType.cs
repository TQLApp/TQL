using Launcher.Abstractions;
using Launcher.Utilities;

namespace Launcher.Plugins.AzureDevOps.Categories;

internal class BacklogType : IMatchType
{
    public static readonly MatchTypeId TypeId =
        new(Guid.Parse("6d8e941f-1181-453f-b298-7c063e0a8da4"), AzureDevOpsPlugin.Id);

    private readonly Images _images;

    public Guid Id => TypeId.Id;

    public BacklogType(Images images)
    {
        _images = images;
    }

    public IMatch Deserialize(string json)
    {
        return new BacklogMatch(JsonSerializer.Deserialize<BacklogMatchDto>(json)!, _images);
    }
}
