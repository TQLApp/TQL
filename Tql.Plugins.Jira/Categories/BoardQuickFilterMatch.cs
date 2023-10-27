using Microsoft.Extensions.DependencyInjection;
using Tql.Abstractions;
using Tql.Plugins.Jira.Data;
using Tql.Plugins.Jira.Services;
using Tql.Plugins.Jira.Support;

namespace Tql.Plugins.Jira.Categories;

internal class BoardQuickFilterMatch
    : ISearchableMatch,
        IHasSearchHint,
        IRunnableMatch,
        ICopyableMatch,
        ISerializableMatch
{
    private readonly BoardQuickFilterMatchDto _dto;
    private readonly IconCacheManager _iconCacheManager;
    private readonly ICache<JiraData> _cache;
    private readonly ConfigurationManager _configurationManager;

    public string Text => $"{_dto.Board.Name}/{_dto.Board.MatchType}/{_dto.Name}";
    public ImageSource Icon { get; }
    public MatchTypeId TypeId => TypeIds.BoardQuickFilter;
    public string SearchHint => "Find issues";

    public BoardQuickFilterMatch(
        BoardQuickFilterMatchDto dto,
        IconCacheManager iconCacheManager,
        ICache<JiraData> cache,
        ConfigurationManager configurationManager
    )
    {
        _dto = dto;
        _iconCacheManager = iconCacheManager;
        _cache = cache;
        _configurationManager = configurationManager;

        Icon = iconCacheManager.GetIcon(dto.Board.AvatarUrl) ?? Images.Boards;
    }

    public async Task Run(IServiceProvider serviceProvider, Window owner)
    {
        serviceProvider.GetRequiredService<IUI>().OpenUrl(await GetUrl());
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(_dto);
    }

    public async Task Copy(IServiceProvider serviceProvider)
    {
        serviceProvider.GetRequiredService<IClipboard>().CopyUri(Text, await GetUrl());
    }

    private async Task<string> GetUrl()
    {
        var cache = await _cache.Get();

        var url = BoardUtils.GetUrl(cache, _dto.Board);

        return $"{url}?quickFilter={_dto.Id}";
    }

    public async Task<IEnumerable<IMatch>> Search(
        ISearchContext context,
        string text,
        CancellationToken cancellationToken
    )
    {
        if (text.IsWhiteSpace())
            return Array.Empty<IMatch>();

        await context.DebounceDelay(cancellationToken);

        var cache = await _cache.Get();
        var connection = cache.GetConnection(_dto.Board.Url);
        var board = connection.Boards.Single(p => p.Id == _dto.Board.Id);
        var quickFilter = board.QuickFilters.Single(p => p.Id == _dto.Id);

        var client = _configurationManager.GetClient(_dto.Board.Url);

        var issues = await client.SearchIssues(
            $"project = \"{board.ProjectKey}\" and {quickFilter.Query} and text ~ \"{text.Replace("\"", "\\\"")}*\"",
            100,
            cancellationToken
        );

        return await IssueUtils.CreateMatches(_dto.Board.Url, issues, client, _iconCacheManager);
    }
}

internal record BoardQuickFilterMatchDto(BoardMatchDto Board, int Id, string Name);
