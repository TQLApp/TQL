using Launcher.Abstractions;
using Launcher.Utilities;

namespace Launcher.Plugins.AzureDevOps.Categories;

internal class WorkItemType : IMatchType
{
    public Guid Id => TypeIds.WorkItem.Id;

    public IMatch Deserialize(string json)
    {
        return new WorkItemMatch(JsonSerializer.Deserialize<WorkItemMatchDto>(json)!);
    }
}
