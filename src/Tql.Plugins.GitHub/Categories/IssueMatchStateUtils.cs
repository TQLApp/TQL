using Octokit;

namespace Tql.Plugins.GitHub.Categories;

internal static class IssueMatchStateUtils
{
    public static IssueMatchState FromIssue(Issue issue)
    {
        if (issue.State.Value == ItemState.Open)
            return IssueMatchState.Open;
        if (issue?.PullRequest.Merged == true)
            return IssueMatchState.Merged;
        return IssueMatchState.Closed;
    }
}
