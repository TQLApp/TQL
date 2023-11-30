using Tql.Abstractions;
using Tql.Plugins.Jira.Services;

namespace Tql.Plugins.Jira.Categories;

internal static class IssueUtils
{
    public static IEnumerable<IMatch> CreateMatches(
        string url,
        IEnumerable<JiraIssueDto> issues,
        IMatchFactory<IssueMatch, IssueMatchDto> factory
    )
    {
        return issues.Select(
            p =>
                factory.Create(
                    new IssueMatchDto(url, p.Key, p.Fields.Summary, p.Fields.IssueType.IconUrl)
                )
        );
    }
}
