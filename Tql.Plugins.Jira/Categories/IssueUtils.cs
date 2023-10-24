using Tql.Abstractions;
using Tql.Plugins.Jira.Services;

namespace Tql.Plugins.Jira.Categories;

internal static class IssueUtils
{
    public static async Task<IEnumerable<IMatch>> CreateMatches(
        string url,
        IEnumerable<JiraIssueDto> issues,
        JiraClient client,
        IconCacheManager iconCacheManager
    )
    {
        var result = issues
            .Select(
                p => new IssueMatchDto(url, p.Key, p.Fields.Summary, p.Fields.IssueType.IconUrl)
            )
            .ToList();

        // Seed the icon cache.

        foreach (var icon in result.Select(p => p.IssueTypeIconUrl).Distinct())
        {
            await iconCacheManager.LoadIcon(client, icon);
        }

        return result.Select(p => new IssueMatch(p, iconCacheManager));
    }
}
