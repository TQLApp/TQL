using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using Tql.Abstractions;
using Tql.Plugins.AzureDevOps.Data;
using Tql.Plugins.AzureDevOps.Services;
using Tql.Utilities;

namespace Tql.Plugins.AzureDevOps.Categories;

internal class BacklogMatch(
    BacklogMatchDto dto,
    ICache<AzureData> cache,
    AzureDevOpsApi api,
    IMatchFactory<WorkItemMatch, WorkItemMatchDto> factory
) : IRunnableMatch, ISerializableMatch, ICopyableMatch, ISearchableMatch
{
    public string Text =>
        MatchText.Path(
            dto.ProjectName,
            dto.TeamName,
            string.Format(Labels.BacklogMatch_Label, dto.BacklogName)
        );

    public ImageSource Icon => Images.Boards;
    public MatchTypeId TypeId => TypeIds.Backlog;
    public string SearchHint => Labels.BacklogMatch_SearchHint;

    public Task Run(IServiceProvider serviceProvider, IWin32Window owner)
    {
        serviceProvider.GetRequiredService<IUI>().OpenUrl(dto.GetUrl());

        return Task.CompletedTask;
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(dto);
    }

    public Task Copy(IServiceProvider serviceProvider)
    {
        serviceProvider.GetRequiredService<IClipboard>().CopyUri(Text, dto.GetUrl());

        return Task.CompletedTask;
    }

    public async Task<IEnumerable<IMatch>> Search(
        ISearchContext context,
        string text,
        CancellationToken cancellationToken
    )
    {
        return await QueryUtils.SearchInBacklog(
            dto.Url,
            dto.ProjectName,
            dto.TeamName,
            dto.BacklogName,
            text,
            api,
            await cache.Get(),
            factory,
            cancellationToken
        );
    }
}

internal record BacklogMatchDto(string Url, string ProjectName, string TeamName, string BacklogName)
{
    public string GetUrl() =>
        $"{Url.TrimEnd('/')}/{Uri.EscapeDataString(ProjectName)}/_backlogs/backlog/{Uri.EscapeDataString(TeamName)}/{Uri.EscapeDataString(BacklogName)}";
};
