using Tql.Abstractions;
using Tql.Plugins.AzureDevOps.Services;
using Tql.Utilities;

namespace Tql.Plugins.AzureDevOps.Categories;

internal class WorkItemType : IMatchType
{
    private readonly AzureWorkItemIconManager _iconManager;
    private readonly ConfigurationManager _configurationManager;

    public Guid Id => TypeIds.WorkItem.Id;

    public WorkItemType(
        AzureWorkItemIconManager iconManager,
        ConfigurationManager configurationManager
    )
    {
        _iconManager = iconManager;
        _configurationManager = configurationManager;
    }

    public IMatch? Deserialize(string json)
    {
        var dto = JsonSerializer.Deserialize<WorkItemMatchDto>(json)!;
        if (!_configurationManager.Configuration.HasConnection(dto.Url))
            return null;

        return new WorkItemMatch(dto, _iconManager);
    }
}
