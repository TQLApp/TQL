using Launcher.Abstractions;
using Launcher.Utilities;

namespace Launcher.Plugins.GitHub.Categories;

internal class PullRequestType : IMatchType
{
    public Guid Id => TypeIds.PullRequest.Id;

    public IMatch Deserialize(string json)
    {
        return new IssueMatch(JsonSerializer.Deserialize<IssueMatchDto>(json)!);
    }
}
