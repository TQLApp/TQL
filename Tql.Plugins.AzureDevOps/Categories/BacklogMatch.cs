using Microsoft.Extensions.DependencyInjection;
using Tql.Abstractions;
using Tql.Plugins.AzureDevOps.Data;
using Tql.Plugins.AzureDevOps.Services;
using Tql.Utilities;

namespace Tql.Plugins.AzureDevOps.Categories;

internal class BacklogMatch : IRunnableMatch, ISerializableMatch, ICopyableMatch, ISearchableMatch
{
    private readonly BacklogMatchDto _dto;
    private readonly ICache<AzureData> _cache;
    private readonly AzureDevOpsApi _api;
    private readonly IMatchFactory<WorkItemMatch, WorkItemMatchDto> _factory;

    public string Text =>
        MatchText.Path(
            _dto.ProjectName,
            _dto.TeamName,
            string.Format(Labels.BacklogMatch_Label, _dto.BacklogName)
        );

    public ImageSource Icon => Images.Boards;
    public MatchTypeId TypeId => TypeIds.Backlog;
    public string SearchHint => Labels.BacklogMatch_SearchHint;

    public BacklogMatch(
        BacklogMatchDto dto,
        ICache<AzureData> cache,
        AzureDevOpsApi api,
        IMatchFactory<WorkItemMatch, WorkItemMatchDto> factory
    )
    {
        _dto = dto;
        _cache = cache;
        _api = api;
        _factory = factory;
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
            _dto.BacklogName,
            text,
            _api,
            await _cache.Get(),
            _factory,
            cancellationToken
        );
    }
}

internal record BacklogMatchDto(string Url, string ProjectName, string TeamName, string BacklogName)
{
    public string GetUrl() =>
        $"{Url.TrimEnd('/')}/{Uri.EscapeDataString(ProjectName)}/_backlogs/backlog/{Uri.EscapeDataString(TeamName)}/{Uri.EscapeDataString(BacklogName)}";
};
