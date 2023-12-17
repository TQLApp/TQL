using Octokit;
using Tql.Abstractions;

namespace Tql.Plugins.GitHub.Categories;

internal class IssueMatch(IssueMatchDto dto) : IssueMatchBase(dto)
{
    public override ImageSource Icon =>
        Dto.State == IssueMatchState.Open ? Images.OpenIssue : Images.ClosedIssue;
    public override MatchTypeId TypeId => TypeIds.Issue;
}
