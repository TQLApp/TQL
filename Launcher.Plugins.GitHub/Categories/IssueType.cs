using Launcher.Abstractions;
using Launcher.Utilities;

namespace Launcher.Plugins.GitHub.Categories;

internal class IssueType : IMatchType
{
    public Guid Id => TypeIds.Issue.Id;

    public IMatch Deserialize(string json)
    {
        return new IssueMatch(JsonSerializer.Deserialize<IssueMatchDto>(json)!);
    }
}
