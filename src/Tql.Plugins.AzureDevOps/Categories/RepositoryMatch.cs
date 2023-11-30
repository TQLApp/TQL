using System.Text.Json.Nodes;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Tql.Abstractions;
using Tql.Plugins.AzureDevOps.Services;
using Tql.Plugins.AzureDevOps.Support;
using Tql.Utilities;

namespace Tql.Plugins.AzureDevOps.Categories;

internal class RepositoryMatch(
    RepositoryMatchDto dto,
    IMatchFactory<RepositoryFilePathMatch, RepositoryFilePathMatchDto> factory
) : IRunnableMatch, ISearchableMatch, ISerializableMatch, ICopyableMatch
{
    private const int MaxResults = 100;

    public string Text => MatchText.Path(dto.ProjectName, dto.RepositoryName);
    public ImageSource Icon => Images.Repositories;
    public MatchTypeId TypeId => TypeIds.Repository;
    public string SearchHint => Labels.RepositoryMatch_FindFiles;

    public async Task<IEnumerable<IMatch>> Search(
        ISearchContext context,
        string text,
        CancellationToken cancellationToken
    )
    {
        var cache = context.GetDataCached($"{GetType().FullName}:{dto}", GetRepositoryFilePaths);

        if (!cache.IsCompleted)
            await context.DebounceDelay(cancellationToken);

        return context.Filter(await cache).Take(MaxResults);
    }

    private async Task<ImmutableArray<IMatch>> GetRepositoryFilePaths(
        IServiceProvider serviceProvider
    )
    {
        var gitClient = await serviceProvider
            .GetRequiredService<AzureDevOpsApi>()
            .GetClient<GitHttpClient>(dto.Url);

        using var stream = await gitClient
            .GetClient()
            .GetStreamAsync(
                $"{dto.ProjectName}/_apis/git/repositories/{dto.RepositoryName}/FilePaths"
            );

        var node = JsonNode.Parse(stream);

        return node!.AsObject()["paths"]!
            .AsArray()
            .Select(p => factory.Create(new RepositoryFilePathMatchDto(dto, p!.GetValue<string>())))
            .ToImmutableArray<IMatch>();
    }

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
}

internal record RepositoryMatchDto(string Url, string ProjectName, string RepositoryName)
{
    public string GetUrl() =>
        $"{Url.TrimEnd('/')}/{Uri.EscapeDataString(ProjectName)}/_git/{Uri.EscapeDataString(RepositoryName)}";
};
