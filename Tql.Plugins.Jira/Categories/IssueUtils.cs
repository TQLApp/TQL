using Tql.Abstractions;
using Tql.Plugins.Jira.Services;

namespace Tql.Plugins.Jira.Categories;

internal static class IssueUtils
{
    public static IEnumerable<IMatch> CreateMatches(
        string url,
        IEnumerable<JiraIssueDto> issues,
        IconCacheManager iconCacheManager
    )
    {
        return issues.Select(
            p =>
                new IssueMatch(
                    new IssueMatchDto(url, p.Key, p.Fields.Summary, p.Fields.IssueType.IconUrl),
                    iconCacheManager
                )
        );
    }
}
