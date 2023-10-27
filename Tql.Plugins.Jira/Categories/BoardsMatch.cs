using Microsoft.Extensions.Logging;
using Tql.Abstractions;
using Tql.Plugins.Jira.Data;
using Tql.Plugins.Jira.Services;
using Tql.Plugins.Jira.Support;
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
        // Download the Board avatars in the background.

        var boards = data.GetConnection(_url).Boards;

        TaskUtils.RunBackground(async () =>
        {
            var client = _configurationManager.GetClient(_url);

            foreach (var board in boards)
            {
                try
                {
                    await _iconCacheManager.LoadIcon(client, board.AvatarUrl);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(
                        ex,
                        "Failed to download Board avatar '{Url}'",
                        board.AvatarUrl
                    );
                }
            }
        });

        foreach (var matchType in EnumEx.GetValues<BoardMatchType>())
        {
            foreach (var board in boards)
            {
                if (matchType == BoardMatchType.Backlog && !board.IsIssueListBacklog)
                    continue;

                yield return new BoardMatch(
                    new BoardMatchDto(
                        _url,
                        board.Id,
                        board.Name,
                        board.ProjectKey,
                        board.ProjectTypeKey,
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
