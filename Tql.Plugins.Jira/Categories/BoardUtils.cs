using Tql.Plugins.Jira.Data;

namespace Tql.Plugins.Jira.Categories;

internal static class BoardUtils
{
    public static string GetUrl(BoardMatchDto dto)
    {
        var url = $"{dto.Url.TrimEnd('/')}/jira/{Uri.EscapeDataString(dto.ProjectTypeKey)}";
        if (
            dto.ProjectType == BoardProjectType.ClassicKanban
            || dto.ProjectType == BoardProjectType.ClassicScrum
        )
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

    public static string GetLabel(BoardMatchDto dto)
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
