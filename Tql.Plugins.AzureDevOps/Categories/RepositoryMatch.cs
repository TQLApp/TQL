using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Tql.Abstractions;
using Tql.Plugins.AzureDevOps.Services;
using Tql.Plugins.AzureDevOps.Support;
using Tql.Utilities;

namespace Tql.Plugins.AzureDevOps.Categories;

internal class RepositoryMatch
    : IRunnableMatch,
        ISearchableMatch,
        ISerializableMatch,
        ICopyableMatch,
        IHasSearchHint
{
    private const int MaxResults = 100;

    private readonly RepositoryMatchDto _dto;
    private readonly IMatchFactory<RepositoryFilePathMatch, RepositoryFilePathMatchDto> _factory;

    public string Text => MatchText.Path(_dto.ProjectName, _dto.RepositoryName);
    public ImageSource Icon => Images.Repositories;
    public MatchTypeId TypeId => TypeIds.Repository;
    public string SearchHint => Labels.RepositoryMatch_FindFiles;

    public RepositoryMatch(
        RepositoryMatchDto dto,
        IMatchFactory<RepositoryFilePathMatch, RepositoryFilePathMatchDto> factory
    )
    {
        _dto = dto;
        _factory = factory;
    }

    public async Task<IEnumerable<IMatch>> Search(
        ISearchContext context,
        string text,
        CancellationToken cancellationToken
    )
    {
        var cache = context.GetDataCached($"{GetType().FullName}:{_dto}", GetRepositoryFilePaths);

        if (!cache.IsCompleted)
            await context.DebounceDelay(cancellationToken);

        var filePaths = await cache;

        return await context.FilterAsync(
            filePaths.Select(p => _factory.Create(new RepositoryFilePathMatchDto(_dto, p))),
            MaxResults
        );
    }

    private async Task<ImmutableArray<string>> GetRepositoryFilePaths(
        IServiceProvider serviceProvider
    )
    {
        var gitClient = await serviceProvider
            .GetRequiredService<AzureDevOpsApi>()
            .GetClient<GitHttpClient>(_dto.Url);

        using var stream = await gitClient
            .GetClient()
            .GetStreamAsync(
                $"{_dto.ProjectName}/_apis/git/repositories/{_dto.RepositoryName}/FilePaths"
            );

        var node = JsonNode.Parse(stream);

        return node!.AsObject()["paths"]!
            .AsArray()
            .Select(p => p!.GetValue<string>())
            .ToImmutableArray();
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
}

internal record RepositoryMatchDto(string Url, string ProjectName, string RepositoryName)
{
    public string GetUrl() =>
        $"{Url.TrimEnd('/')}/{Uri.EscapeDataString(ProjectName)}/_git/{Uri.EscapeDataString(RepositoryName)}";
};
