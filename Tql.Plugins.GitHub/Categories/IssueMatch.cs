using Octokit;
using Tql.Abstractions;

namespace Tql.Plugins.GitHub.Categories;

internal class IssueMatch : IssueMatchBase
{
    public override ImageSource Icon =>
        Dto.State == ItemState.Open ? Images.OpenIssue : Images.ClosedIssue;
    public override MatchTypeId TypeId => TypeIds.Issue;

    public IssueMatch(IssueMatchDto dto)
        : base(dto) { }
}
