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
    private readonly IconCacheManager _iconCacheManager;
    private readonly JiraApi _api;
    private readonly ILogger<BoardsMatch> _logger;

    public override string Text { get; }
    public override ImageSource Icon => Images.Boards;
    public override MatchTypeId TypeId => TypeIds.Boards;

    public BoardsMatch(
        string text,
        string url,
        ICache<JiraData> cache,
        IconCacheManager iconCacheManager,
        JiraApi api,
        ILogger<BoardsMatch> logger
    )
        : base(cache)
    {
        _url = url;
        _iconCacheManager = iconCacheManager;
        _api = api;
        _logger = logger;

        Text = text;
    }

    protected override IEnumerable<IMatch> Create(JiraData data)
    {
        // Download the Board avatars in the background.

        var boards = data.GetConnection(_url).Boards;

        TaskUtils.RunBackground(async () =>
        {
            var client = _api.GetClient(_url);

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

        return from matchType in EnumEx.GetValues<BoardMatchType>()
            from board in boards
            select new BoardMatch(
                new BoardMatchDto(
                    _url,
                    board.Id,
                    board.Name,
                    board.ProjectKey,
                    board.ProjectTypeKey,
                    board.AvatarUrl,
                    matchType
                ),
                _iconCacheManager
            );
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(new RootItemDto(_url));
    }
}
