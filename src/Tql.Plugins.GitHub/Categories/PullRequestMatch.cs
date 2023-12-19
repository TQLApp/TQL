using Tql.Abstractions;

namespace Tql.Plugins.GitHub.Categories;

internal class PullRequestMatch(IssueMatchDto dto) : IssueMatchBase(dto)
{
    public override ImageSource Icon =>
        Dto.State switch
        {
            IssueMatchState.Open => Images.OpenPullRequest,
            IssueMatchState.Closed => Images.ClosedPullRequest,
            IssueMatchState.Merged => Images.MergedPullRequest,
            _ => throw new ArgumentOutOfRangeException()
        };

    public override MatchTypeId TypeId => TypeIds.PullRequest;
}
