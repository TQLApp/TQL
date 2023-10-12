using Launcher.Abstractions;
using Launcher.Plugins.AzureDevOps.Services;
using Launcher.Utilities;

namespace Launcher.Plugins.AzureDevOps.Categories;

internal class WorkItemType : IMatchType
{
    private readonly AzureWorkItemIconManager _iconManager;
    public Guid Id => TypeIds.WorkItem.Id;

    public WorkItemType(AzureWorkItemIconManager iconManager)
    {
        _iconManager = iconManager;
    }

    public IMatch Deserialize(string json)
    {
        return new WorkItemMatch(JsonSerializer.Deserialize<WorkItemMatchDto>(json)!, _iconManager);
    }
}
