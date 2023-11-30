using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Tql.Abstractions;
using Tql.Plugins.AzureDevOps.Services;
using Tql.Utilities;

namespace Tql.Plugins.AzureDevOps.Categories;

internal class QueryMatch(
    QueryMatchDto dto,
    AzureDevOpsApi api,
    IMatchFactory<WorkItemMatch, WorkItemMatchDto> factory
) : IRunnableMatch, ISerializableMatch, ICopyableMatch, ISearchableMatch
{
    public string Text => MatchText.Path(dto.ProjectName, dto.Path.Trim('\\').Replace('\\', '/'));

    public ImageSource Icon => Images.Boards;
    public MatchTypeId TypeId => TypeIds.Query;
    public string SearchHint => Labels.QueryMatch_SearchHint;

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
        var cache = context.GetDataCached($"{GetType().FullName}:{dto}", ExecuteQuery);

        if (!cache.IsCompleted)
            await context.DebounceDelay(cancellationToken);

        return context.Filter(await cache);
    }

    private async Task<ImmutableArray<IMatch>> ExecuteQuery(IServiceProvider serviceProvider)
    {
        var client = await api.GetClient<WorkItemTrackingHttpClient>(dto.Url);

        // GetWorkItemsAsync allows for a maximum of 200 work items.
        // Limit to that.
        var result = await client.QueryByIdAsync(dto.Id, top: 200);

        return await QueryUtils.GetWorkItemsByIds(client, dto.Url, result, factory);
    }
}

internal record QueryMatchDto(string Url, string ProjectName, Guid Id, string Path, string Name)
{
    public string GetUrl() =>
        $"{Url.TrimEnd('/')}/{Uri.EscapeDataString(ProjectName)}/_queries/query-edit/{Uri.EscapeDataString(Id.ToString())}";
};
