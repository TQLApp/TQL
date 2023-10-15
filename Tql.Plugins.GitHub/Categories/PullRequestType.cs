using Tql.Abstractions;
using Tql.Utilities;

namespace Tql.Plugins.GitHub.Categories;

internal class PullRequestType : IMatchType
{
    public Guid Id => TypeIds.PullRequest.Id;

    public IMatch Deserialize(string json)
    {
        return new IssueMatch(JsonSerializer.Deserialize<IssueMatchDto>(json)!);
    }
}
