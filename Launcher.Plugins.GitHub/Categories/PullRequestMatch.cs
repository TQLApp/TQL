using Launcher.Abstractions;

namespace Launcher.Plugins.GitHub.Categories;

internal class PullRequestMatch : IssueMatchBase
{
    public override ImageSource Icon => Images.PullRequest;
    public override MatchTypeId TypeId => TypeIds.PullRequest;

    public PullRequestMatch(IssueMatchDto dto)
        : base(dto) { }
}
