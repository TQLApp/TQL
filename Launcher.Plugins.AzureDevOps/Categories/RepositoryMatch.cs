using Launcher.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace Launcher.Plugins.AzureDevOps.Categories;

internal class RepositoryMatch : IRunnableMatch, ISearchableMatch, ISerializableMatch
{
    private readonly Images _images;
    private readonly RepositoryDto _dto;

    public string Text => $"{_dto.ProjectName}/{_dto.RepositoryName}";
    public IImage Icon => _images.Repositories;

    public RepositoryMatch(Images images, RepositoryDto dto)
    {
        _images = images;
        _dto = dto;
    }

    public Task<IEnumerable<IMatch>> Search(
        ISearchContext context,
        string text,
        CancellationToken cancellationToken
    )
    {
        throw new NotImplementedException();
    }

    public Task Run(IServiceProvider serviceProvider, Window owner)
    {
        serviceProvider
            .GetRequiredService<IUI>()
            .LaunchUrl($"{_dto.Url.TrimEnd('/')}/{_dto.ProjectName}/_git/{_dto.RepositoryName}");

        return Task.CompletedTask;
    }

    public (Guid TypeId, string Json) Serialize()
    {
        return (RepositoryType.Id, JsonSerializer.Serialize(_dto));
    }
}

internal record RepositoryDto(string Url, string ProjectName, string RepositoryName);
