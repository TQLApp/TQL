using Launcher.Abstractions;
using Launcher.Utilities;

namespace Launcher.Plugins.AzureDevOps.Categories;

internal class WorkItemType : IMatchType
{
    private readonly Images _images;

    public Guid Id => TypeIds.WorkItem.Id;

    public WorkItemType(Images images)
    {
        _images = images;
    }

    public IMatch Deserialize(string json)
    {
        return new WorkItemMatch(JsonSerializer.Deserialize<WorkItemMatchDto>(json)!, _images);
    }
}
