using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using Tql.Abstractions;
using Tql.Plugins.Jira.Data;
using Tql.Plugins.Jira.Services;
using Tql.Plugins.Jira.Support;
using Tql.Utilities;

namespace Tql.Plugins.Jira.Categories;

internal class BoardMatch(
    BoardMatchDto dto,
    IconCacheManager iconCacheManager,
    ICache<JiraData> cache,
    IMatchFactory<BoardQuickFilterMatch, BoardQuickFilterMatchDto> factory
) : IRunnableMatch, ISerializableMatch, ICopyableMatch, ISearchableMatch
{
    public string Text => MatchText.Path(dto.Name, MatchUtils.GetBoardLabel(dto));
    public ImageSource Icon { get; } =
        iconCacheManager.GetIcon(new IconKey(dto.Url, dto.AvatarUrl)) ?? Images.Boards;
    public MatchTypeId TypeId => TypeIds.Board;
    public string SearchHint => Labels.BoardMatch_SearchHint;

    public Task Run(IServiceProvider serviceProvider, IWin32Window owner)
    {
        serviceProvider.GetRequiredService<IUI>().OpenUrl(MatchUtils.GetBoardUrl(dto));

        return Task.CompletedTask;
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(dto);
    }

    public Task Copy(IServiceProvider serviceProvider)
    {
        serviceProvider.GetRequiredService<IClipboard>().CopyUri(Text, MatchUtils.GetBoardUrl(dto));

        return Task.CompletedTask;
    }

    public async Task<IEnumerable<IMatch>> Search(
        ISearchContext context,
        string text,
        CancellationToken cancellationToken
    )
    {
        var data = await cache.Get();
        var connection = data.GetConnection(dto.Url);

        var board = connection.Boards.Single(p => p.Id == dto.Id);

        var matches = new List<BoardQuickFilterMatch>
        {
            factory.Create(new BoardQuickFilterMatchDto(dto, null, Labels.BoardMatch_AllIssues))
        };

        matches.AddRange(
            board.QuickFilters.Select(p =>
                factory.Create(new BoardQuickFilterMatchDto(dto, p.Id, p.Name))
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
