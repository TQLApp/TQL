using Tql.Abstractions;
using Tql.Plugins.AzureDevOps.Data;
using Tql.Plugins.AzureDevOps.Services;
using Tql.Utilities;

namespace Tql.Plugins.AzureDevOps.Categories;

internal class BoardsMatch : CachedMatch<AzureData>, ISerializableMatch
{
    private readonly string _url;
    private readonly ICache<AzureData> _cache;
    private readonly AzureDevOpsApi _api;
    private readonly AzureWorkItemIconManager _iconManager;

    public override string Text { get; }
    public override ImageSource Icon => Images.Boards;
    public override MatchTypeId TypeId => TypeIds.Boards;

    public BoardsMatch(
        string text,
        string url,
        ICache<AzureData> cache,
        AzureDevOpsApi api,
        AzureWorkItemIconManager iconManager
    )
        : base(cache)
    {
        _url = url;
        _cache = cache;
        _api = api;
        _iconManager = iconManager;

        Text = text;
    }

    protected override IEnumerable<IMatch> Create(AzureData data)
    {
        return from project in data.GetConnection(_url).Projects
            from team in project.Teams
            from board in project.Boards
            select new BoardMatch(
                new BoardMatchDto(_url, project.Name, team.Name, board.Name),
                _cache,
                _api,
                _iconManager
            );
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(new RootItemDto(_url));
    }
}
