using Tql.Abstractions;
using Tql.Plugins.AzureDevOps.Services;
using Tql.Utilities;

namespace Tql.Plugins.AzureDevOps.Categories;

internal class NewPullRequestsType(
    IMatchFactory<NewPullRequestsMatch, NewPullRequestsMatchDto> factory,
    ConfigurationManager configurationManager
) : MatchType<NewPullRequestsMatch, NewPullRequestsMatchDto>(factory)
{
    public override Guid Id => TypeIds.NewPullRequests.Id;

    protected override bool IsValid(NewPullRequestsMatchDto dto) =>
        configurationManager.Configuration.HasConnection(dto.Url);
}
