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
    private readonly IconCacheManager _iconCacheManager;
    private readonly ICache<JiraData> _cache;
    private readonly ConfigurationManager _configurationManager;

    public string Text => MatchText.Path(_dto.Name, MatchUtils.GetBoardLabel(_dto));
    public ImageSource Icon { get; }
    public MatchTypeId TypeId => TypeIds.Board;

    public BoardMatch(
        BoardMatchDto dto,
        IconCacheManager iconCacheManager,
        ICache<JiraData> cache,
        ConfigurationManager configurationManager
    )
    {
        _dto = dto;
        _iconCacheManager = iconCacheManager;
        _cache = cache;
        _configurationManager = configurationManager;
        Icon = iconCacheManager.GetIcon(new IconKey(dto.Url, dto.AvatarUrl)) ?? Images.Boards;
    }

    public Task Run(IServiceProvider serviceProvider, Window owner)
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
            new(
                new BoardQuickFilterMatchDto(_dto, null, Labels.BoardMatch_AllIssues),
                _iconCacheManager,
                _cache,
                _configurationManager
            )
        };

        matches.AddRange(
            board.QuickFilters.Select(
                p =>
                    new BoardQuickFilterMatch(
                        new BoardQuickFilterMatchDto(_dto, p.Id, p.Name),
                        _iconCacheManager,
                        _cache,
                        _configurationManager
                    )
            )
        );

        return await context.FilterAsync(matches);
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
