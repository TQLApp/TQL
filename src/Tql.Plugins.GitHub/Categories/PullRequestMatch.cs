using Tql.Abstractions;

namespace Tql.Plugins.GitHub.Categories;

internal class PullRequestMatch(IssueMatchDto dto) : IssueMatchBase(dto)
{
    public override ImageSource Icon => Images.PullRequest;
    public override MatchTypeId TypeId => TypeIds.PullRequest;
}
