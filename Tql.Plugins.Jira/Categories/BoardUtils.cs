using Tql.Abstractions;
using Tql.Plugins.Jira.Data;

namespace Tql.Plugins.Jira.Categories;

internal static class BoardUtils
{
    public static string GetUrl(JiraData cache, BoardMatchDto dto)
    {
        var project = cache
            .GetConnection(dto.Url)
            .Projects.Single(
                p => string.Equals(p.Key, dto.ProjectKey, StringComparison.OrdinalIgnoreCase)
            );

        var url = $"{dto.Url.TrimEnd('/')}/jira/{Uri.EscapeDataString(dto.ProjectTypeKey)}";
        if (string.Equals(project.Style, "classic", StringComparison.OrdinalIgnoreCase))
            url += "/c";
        url += $"/projects/{Uri.EscapeDataString(dto.ProjectKey)}/boards/{dto.Id}";

        switch (dto.MatchType)
        {
            case BoardMatchType.Backlog:
                url += "/backlog";
                break;
            case BoardMatchType.Timeline:
                url += "/timeline";
                break;
        }

        return url;
    }
}
