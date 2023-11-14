using Microsoft.Extensions.Logging;
using Tql.Abstractions;
using Tql.Plugins.Jira.Data;
using Tql.Plugins.Jira.Services;
using Tql.Plugins.Jira.Support;
using Tql.Utilities;

namespace Tql.Plugins.Jira.Categories;

internal class BoardsMatch : CachedMatch<JiraData>, ISerializableMatch
{
    private readonly RootItemDto _dto;
    private readonly ILogger<BoardsMatch> _logger;
    private readonly ConfigurationManager _configurationManager;
    private readonly IMatchFactory<BoardMatch, BoardMatchDto> _factory;

    public override string Text =>
        MatchUtils.GetMatchLabel(
            Labels.BoardsType_Label,
            _configurationManager.Configuration,
            _dto.Url
        );

    public override ImageSource Icon => Images.Boards;
    public override MatchTypeId TypeId => TypeIds.Boards;

    public BoardsMatch(
        RootItemDto dto,
        ICache<JiraData> cache,
        ILogger<BoardsMatch> logger,
        ConfigurationManager configurationManager,
        IMatchFactory<BoardMatch, BoardMatchDto> factory
    )
        : base(cache)
    {
        _dto = dto;
        _logger = logger;
        _configurationManager = configurationManager;
        _factory = factory;
    }

    protected override IEnumerable<IMatch> Create(JiraData data)
    {
        var connection = data.GetConnection(_dto.Url);

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
                    _logger.LogWarning(
                        "No project found with key '{ProjectKey}'",
                        board.ProjectKey
                    );
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

                yield return _factory.Create(
                    new BoardMatchDto(
                        _dto.Url,
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
        return JsonSerializer.Serialize(_dto);
    }
}
