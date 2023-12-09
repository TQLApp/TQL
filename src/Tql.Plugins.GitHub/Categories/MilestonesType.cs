using Tql.Abstractions;
using Tql.Plugins.GitHub.Services;
using Tql.Utilities;

namespace Tql.Plugins.GitHub.Categories;

internal class MilestonesType(
    IMatchFactory<MilestonesMatch, RepositoryItemMatchDto> factory,
    ConfigurationManager configurationManager
) : MatchType<MilestonesMatch, RepositoryItemMatchDto>(factory)
{
    public override Guid Id => TypeIds.Milestones.Id;

    protected override bool IsValid(RepositoryItemMatchDto dto) =>
        configurationManager.Configuration.HasConnection(dto.ConnectionId);
}
