using Tql.Abstractions;
using Tql.Plugins.AzureDevOps.Services;
using Tql.Utilities;

namespace Tql.Plugins.AzureDevOps.Categories;

internal class WorkItemType : MatchType<WorkItemMatch, WorkItemMatchDto>
{
    private readonly ConfigurationManager _configurationManager;

    public override Guid Id => TypeIds.WorkItem.Id;

    public WorkItemType(
        IMatchFactory<WorkItemMatch, WorkItemMatchDto> factory,
        ConfigurationManager configurationManager
    )
        : base(factory)
    {
        _configurationManager = configurationManager;
    }

    protected override bool IsValid(WorkItemMatchDto dto) =>
        _configurationManager.Configuration.HasConnection(dto.Url);
}
