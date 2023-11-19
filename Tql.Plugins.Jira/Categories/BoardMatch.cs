using Microsoft.Extensions.DependencyInjection;
using Tql.Abstractions;
using Tql.Plugins.Jira.Data;
using Tql.Plugins.Jira.Services;
using Tql.Plugins.Jira.Support;
using Tql.Utilities;

namespace Tql.Plugins.Jira.Categories;

internal class BoardMatch : IRunnableMatch, ISerializableMatch, ICopyableMatch, ISearchableMatch
{
    private readonly BoardMatchDto _dto;
    private readonly ICache<JiraData> _cache;
    private readonly IMatchFactory<BoardQuickFilterMatch, BoardQuickFilterMatchDto> _factory;

    public string Text => MatchText.Path(_dto.Name, MatchUtils.GetBoardLabel(_dto));
    public ImageSource Icon { get; }
    public MatchTypeId TypeId => TypeIds.Board;
    public string SearchHint => Labels.BoardMatch_SearchHint;

    public BoardMatch(
        BoardMatchDto dto,
        IconCacheManager iconCacheManager,
        ICache<JiraData> cache,
        IMatchFactory<BoardQuickFilterMatch, BoardQuickFilterMatchDto> factory
    )
    {
        _dto = dto;
        _cache = cache;
        _factory = factory;
        Icon = iconCacheManager.GetIcon(new IconKey(dto.Url, dto.AvatarUrl)) ?? Images.Boards;
    }

    public Task Run(IServiceProvider serviceProvider, IWin32Window owner)
    {
        serviceProvider.GetRequiredService<IUI>().OpenUrl(MatchUtils.GetBoardUrl(_dto));

        return Task.CompletedTask;
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(_dto);
    }

    public Task Copy(IServiceProvider serviceProvider)
    {
        serviceProvider
            .GetRequiredService<IClipboard>()
            .CopyUri(Text, MatchUtils.GetBoardUrl(_dto));

        return Task.CompletedTask;
    }

    public async Task<IEnumerable<IMatch>> Search(
        ISearchContext context,
        string text,
        CancellationToken cancellationToken
    )
    {
        var cache = await _cache.Get();
        var connection = cache.GetConnection(_dto.Url);

        var board = connection.Boards.Single(p => p.Id == _dto.Id);

        var matches = new List<BoardQuickFilterMatch>
        {
            _factory.Create(new BoardQuickFilterMatchDto(_dto, null, Labels.BoardMatch_AllIssues))
        };

        matches.AddRange(
            board.QuickFilters.Select(
                p => _factory.Create(new BoardQuickFilterMatchDto(_dto, p.Id, p.Name))
            )
        );

        return context.Filter(matches);
    }
}

internal record BoardMatchDto(
    string Url,
    int Id,
    string Name,
    string ProjectKey,
    string ProjectTypeKey,
    BoardProjectType ProjectType,
    string AvatarUrl,
    BoardMatchType MatchType
);

internal enum BoardProjectType
{
    TeamManaged,
    ClassicScrum,
    ClassicKanban
}

internal enum BoardMatchType
{
    Backlog,
    Board,
    Timeline
}
