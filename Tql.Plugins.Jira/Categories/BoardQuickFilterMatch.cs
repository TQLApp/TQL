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

    public string Text => $"{_dto.Board.Name} › {BoardUtils.GetLabel(_dto.Board)} › {_dto.Name}";
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

        Icon =
            iconCacheManager.GetIcon(new IconKey(dto.Board.Url, dto.Board.AvatarUrl))
            ?? Images.Boards;
    }

    public Task Run(IServiceProvider serviceProvider, Window owner)
    {
        serviceProvider.GetRequiredService<IUI>().OpenUrl(GetUrl());

        return Task.CompletedTask;
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(_dto);
    }

    public Task Copy(IServiceProvider serviceProvider)
    {
        serviceProvider.GetRequiredService<IClipboard>().CopyUri(Text, GetUrl());

        return Task.CompletedTask;
    }

    private string GetUrl()
    {
        var url = BoardUtils.GetUrl(_dto.Board);
        if (_dto.Id.HasValue)
            url += $"?quickFilter={_dto.Id}";
        return url;
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

        var client = _configurationManager.GetClient(_dto.Board.Url);

        var query = $"filter = \"{board.FilterId}\" and text ~ \"{text.Replace("\"", "\\\"")}*\"";
        if (_dto.Id.HasValue)
        {
            var quickFilter = board.QuickFilters.Single(p => p.Id == _dto.Id);
            query += $" and {quickFilter.Query}";
        }

        var issues = await client.SearchIssues(query, 100, cancellationToken);

        return IssueUtils.CreateMatches(_dto.Board.Url, issues, _iconCacheManager);
    }
}

internal record BoardQuickFilterMatchDto(BoardMatchDto Board, int? Id, string Name);
