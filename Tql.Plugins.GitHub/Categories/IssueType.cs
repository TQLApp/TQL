using Tql.Abstractions;
using Tql.Utilities;

namespace Tql.Plugins.GitHub.Categories;

internal class IssueType : IMatchType
{
    public Guid Id => TypeIds.Issue.Id;

    public IMatch Deserialize(string json)
    {
        return new IssueMatch(JsonSerializer.Deserialize<IssueMatchDto>(json)!);
    }
}
