using Tql.Plugins.Jira.Categories;

namespace Tql.Plugins.Jira.Support;

internal static class MatchUtils
{
    public static string GetMatchLabel(string label, Configuration configuration, string url)
    {
        if (configuration.Connections.Length > 1)
        {
            var connection = configuration.Connections.Single(p => p.Url == url);

            return $"{label} ({connection.Name})";
        }

        return label;
    }

    public static string GetProjectUrl(
        string connectionUrl,
        string key,
        string typeKey,
        bool isClassic
    )
    {
        var url = $"{connectionUrl.TrimEnd('/')}/jira/{Uri.EscapeDataString(typeKey)}";
        if (isClassic)
            url += "/c";
        return url + $"/projects/{Uri.EscapeDataString(key)}";
    }

    public static string GetBoardUrl(BoardMatchDto dto)
    {
        var url = GetProjectUrl(
            dto.Url,
            dto.ProjectKey,
            dto.ProjectTypeKey,
            dto.ProjectType is BoardProjectType.ClassicKanban or BoardProjectType.ClassicScrum
        );

        url += $"/boards/{dto.Id}";

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

    public static string GetBoardLabel(BoardMatchDto dto)
    {
        if (dto.MatchType == BoardMatchType.Board)
        {
            switch (dto.ProjectType)
            {
                case BoardProjectType.ClassicKanban:
                    return "Kanban board";
                case BoardProjectType.ClassicScrum:
                    return "Active sprints";
            }
        }

        return dto.MatchType.ToString();
    }
}
