using Microsoft.Extensions.Logging;
using Tql.Abstractions;
using Tql.Plugins.Jira.Data;
using Tql.Plugins.Jira.Services;
using Tql.Utilities;

namespace Tql.Plugins.Jira.Categories;

internal class BoardsMatch : CachedMatch<JiraData>, ISerializableMatch
{
    private readonly string _url;
    private readonly ICache<JiraData> _cache;
    private readonly IconCacheManager _iconCacheManager;
    private readonly ConfigurationManager _configurationManager;
    private readonly ILogger<BoardsMatch> _logger;

    public override string Text { get; }
    public override ImageSource Icon => Images.Boards;
    public override MatchTypeId TypeId => TypeIds.Boards;

    public BoardsMatch(
        string text,
        string url,
        ICache<JiraData> cache,
        IconCacheManager iconCacheManager,
        ConfigurationManager configurationManager,
        ILogger<BoardsMatch> logger
    )
        : base(cache)
    {
        _url = url;
        _cache = cache;
        _iconCacheManager = iconCacheManager;
        _configurationManager = configurationManager;
        _logger = logger;

        Text = text;
    }

    protected override IEnumerable<IMatch> Create(JiraData data)
    {
        var connection = data.GetConnection(_url);

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

                yield return new BoardMatch(
                    new BoardMatchDto(
                        _url,
                        board.Id,
                        board.Name,
                        board.ProjectKey,
                        board.ProjectTypeKey,
                        projectType,
                        board.AvatarUrl,
                        matchType
                    ),
                    _iconCacheManager,
                    _cache,
                    _configurationManager
                );
            }
        }
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(new RootItemDto(_url));
    }
}
