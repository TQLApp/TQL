using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using Tql.Abstractions;
using Tql.Plugins.Jira.Data;
using Tql.Plugins.Jira.Services;
using Tql.Plugins.Jira.Support;
using Tql.Utilities;

namespace Tql.Plugins.Jira.Categories;

internal class BoardQuickFilterMatch(
    BoardQuickFilterMatchDto dto,
    IconCacheManager iconCacheManager,
    ICache<JiraData> cache,
    ConfigurationManager configurationManager,
    IMatchFactory<IssueMatch, IssueMatchDto> factory
) : ISearchableMatch, IRunnableMatch, ICopyableMatch, ISerializableMatch
{
    public string Text =>
        MatchText.Path(dto.Board.Name, MatchUtils.GetBoardLabel(dto.Board), dto.Name);
    public ImageSource Icon { get; } =
        iconCacheManager.GetIcon(new IconKey(dto.Board.Url, dto.Board.AvatarUrl)) ?? Images.Boards;

    public MatchTypeId TypeId => TypeIds.BoardQuickFilter;
    public string SearchHint => Labels.BoardQuickFilterMatch_SearchHint;

    public Task Run(IServiceProvider serviceProvider, IWin32Window owner)
    {
        serviceProvider.GetRequiredService<IUI>().OpenUrl(GetUrl());

        return Task.CompletedTask;
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(dto);
    }

    public Task Copy(IServiceProvider serviceProvider)
    {
        serviceProvider.GetRequiredService<IClipboard>().CopyUri(Text, GetUrl());

        return Task.CompletedTask;
    }

    private string GetUrl()
    {
        var url = MatchUtils.GetBoardUrl(dto.Board);
        if (dto.Id.HasValue)
            url += $"?quickFilter={dto.Id}";
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

        var data = await cache.Get();
        var connection = data.GetConnection(dto.Board.Url);
        var board = connection.Boards.Single(p => p.Id == dto.Board.Id);

        var client = configurationManager.GetClient(dto.Board.Url);

        var query = $"filter = \"{board.FilterId}\" and text ~ \"{text.Replace("\"", "\\\"")}*\"";
        if (dto.Id.HasValue)
        {
            var quickFilter = board.QuickFilters.Single(p => p.Id == dto.Id);
            query += $" and {quickFilter.Query}";
        }

        var issues = await client.SearchIssues(query, 100, cancellationToken);

        return IssueUtils.CreateMatches(dto.Board.Url, issues, factory);
    }
}

internal record BoardQuickFilterMatchDto(BoardMatchDto Board, int? Id, string Name);
