using Microsoft.Extensions.Logging;
using Tql.Abstractions;
using Tql.Plugins.Jira.Data;
using Tql.Plugins.Jira.Services;
using Tql.Plugins.Jira.Support;
using Tql.Utilities;

namespace Tql.Plugins.Jira.Categories;

internal class BoardsMatch(
    RootItemDto dto,
    ICache<JiraData> cache,
    ILogger<BoardsMatch> logger,
    ConfigurationManager configurationManager,
    IMatchFactory<BoardMatch, BoardMatchDto> factory
) : CachedMatch<JiraData>(cache), ISerializableMatch
{
    public override string Text =>
        MatchUtils.GetMatchLabel(
            Labels.BoardsMatch_Label,
            configurationManager.Configuration,
            dto.Url
        );

    public override ImageSource Icon => Images.Boards;
    public override MatchTypeId TypeId => TypeIds.Boards;
    public override string SearchHint => Labels.BoardsMatch_SearchHint;

    protected override IEnumerable<IMatch> Create(JiraData data)
    {
        var connection = data.GetConnection(dto.Url);

        var boards = connection.Boards;

        foreach (var matchType in EnumEx.GetValues<BoardMatchType>())
        {
            foreach (var board in boards)
            {
                if (matchType == BoardMatchType.Backlog && !board.IsIssueListBacklog)
                    continue;

                var project = connection.Projects.SingleOrDefault(
                    p => string.Equals(p.Key, board.ProjectKey, StringComparison.OrdinalIgnoreCase)
                );
                if (project == null)
                {
                    logger.LogWarning("No project found with key '{ProjectKey}'", board.ProjectKey);
                    continue;
                }

                BoardProjectType projectType;
                if (string.Equals(project.Style, "classic", StringComparison.OrdinalIgnoreCase))
                {
                    projectType = board.IsSprintSupportEnabled
                        ? BoardProjectType.ClassicScrum
                        : BoardProjectType.ClassicKanban;
                }
                else
                {
                    projectType = BoardProjectType.TeamManaged;
                }

                yield return factory.Create(
                    new BoardMatchDto(
                        dto.Url,
                        board.Id,
                        board.Name,
                        board.ProjectKey,
                        board.ProjectTypeKey,
                        projectType,
                        board.AvatarUrl,
                        matchType
                    )
                );
            }
        }
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(dto);
    }
}
