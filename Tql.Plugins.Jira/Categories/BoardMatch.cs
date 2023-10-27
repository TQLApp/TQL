using Microsoft.Extensions.DependencyInjection;
using Tql.Abstractions;
using Tql.Plugins.Jira.Data;
using Tql.Plugins.Jira.Services;

namespace Tql.Plugins.Jira.Categories;

internal class BoardMatch : IRunnableMatch, ISerializableMatch, ICopyableMatch, ISearchableMatch
{
    private readonly BoardMatchDto _dto;
    private readonly IconCacheManager _iconCacheManager;
    private readonly ICache<JiraData> _cache;
    private readonly ConfigurationManager _configurationManager;

    public string Text => $"{_dto.Name}/{_dto.MatchType}";
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

        Icon = iconCacheManager.GetIcon(dto.AvatarUrl) ?? Images.Boards;
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

        return BoardUtils.GetUrl(cache, _dto);
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

        return context.Filter(
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
    }
}

internal record BoardMatchDto(
    string Url,
    int Id,
    string Name,
    string ProjectKey,
    string ProjectTypeKey,
    string AvatarUrl,
    BoardMatchType MatchType
);

internal enum BoardMatchType
{
    Backlog,
    Board,
    Timeline
}
