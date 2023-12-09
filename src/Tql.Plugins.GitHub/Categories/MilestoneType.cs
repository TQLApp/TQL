using Tql.Abstractions;
using Tql.Plugins.GitHub.Services;
using Tql.Utilities;

namespace Tql.Plugins.GitHub.Categories;

internal class MilestoneType(
    IMatchFactory<MilestoneMatch, MilestoneMatchDto> factory,
    ConfigurationManager configurationManager
) : MatchType<MilestoneMatch, MilestoneMatchDto>(factory)
{
    public override Guid Id => TypeIds.Milestone.Id;

    protected override bool IsValid(MilestoneMatchDto dto) =>
        configurationManager.Configuration.HasConnection(dto.ConnectionId);
}
