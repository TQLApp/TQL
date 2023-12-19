using Tql.Abstractions;
using Tql.Plugins.GitHub.Services;
using Tql.Utilities;

namespace Tql.Plugins.GitHub.Categories;

internal class NewPullRequestsType(
    IMatchFactory<NewPullRequestsMatch, NewMatchDto> factory,
    ConfigurationManager configurationManager
) : MatchType<NewPullRequestsMatch, NewMatchDto>(factory)
{
    public override Guid Id => TypeIds.NewPullRequests.Id;

    protected override bool IsValid(NewMatchDto dto) =>
        !dto.Id.HasValue || configurationManager.Configuration.HasConnection(dto.Id.Value);
}
