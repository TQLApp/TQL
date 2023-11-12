using Microsoft.Extensions.DependencyInjection;
using Tql.Abstractions;
using Tql.Plugins.AzureDevOps.Data;
using Tql.Plugins.AzureDevOps.Services;
using Tql.Utilities;

namespace Tql.Plugins.AzureDevOps.Categories;

internal class BoardMatch
    : IRunnableMatch,
        ISerializableMatch,
        ICopyableMatch,
        ISearchableMatch,
        IHasSearchHint
{
    private readonly BoardMatchDto _dto;
    private readonly ICache<AzureData> _cache;
    private readonly AzureDevOpsApi _api;
    private readonly AzureWorkItemIconManager _iconManager;

    public string Text =>
        MatchText.Path(
            _dto.ProjectName,
            _dto.TeamName,
            string.Format(Labels.BoardMatch_Label, _dto.BoardName)
        );

    public ImageSource Icon => Images.Boards;
    public MatchTypeId TypeId => TypeIds.Board;
    public string SearchHint => Labels.BoardMatch_SearchHint;

    public BoardMatch(
        BoardMatchDto dto,
        ICache<AzureData> cache,
        AzureDevOpsApi api,
        AzureWorkItemIconManager iconManager
    )
    {
        _dto = dto;
        _cache = cache;
        _api = api;
        _iconManager = iconManager;
    }

    public Task Run(IServiceProvider serviceProvider, Window owner)
    {
        serviceProvider.GetRequiredService<IUI>().OpenUrl(_dto.GetUrl());

        return Task.CompletedTask;
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(_dto);
    }

    public Task Copy(IServiceProvider serviceProvider)
    {
        serviceProvider.GetRequiredService<IClipboard>().CopyUri(Text, _dto.GetUrl());

        return Task.CompletedTask;
    }

    public async Task<IEnumerable<IMatch>> Search(
        ISearchContext context,
        string text,
        CancellationToken cancellationToken
    )
    {
        return await QueryUtils.SearchInBacklog(
            _dto.Url,
            _dto.ProjectName,
            _dto.TeamName,
            _dto.BoardName,
            text,
            _api,
            await _cache.Get(),
            _iconManager,
            cancellationToken
        );
    }
}

internal record BoardMatchDto(string Url, string ProjectName, string TeamName, string BoardName)
{
    public string GetUrl() =>
        $"{Url.TrimEnd('/')}/{Uri.EscapeDataString(ProjectName)}/_boards/board/t/{Uri.EscapeDataString(TeamName)}/{Uri.EscapeDataString(BoardName)}";
};
