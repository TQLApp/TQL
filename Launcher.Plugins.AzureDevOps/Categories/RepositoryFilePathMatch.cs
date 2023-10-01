using Launcher.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Launcher.Plugins.AzureDevOps.Categories;

internal class RepositoryFilePathMatch : IRunnableMatch, ISerializableMatch
{
    private readonly RepositoryFilePathMatchDto _dto;
    private readonly Images _images;

    public string Text => _dto.FilePath;
    public IImage Icon => _images.Document;

    public RepositoryFilePathMatch(RepositoryFilePathMatchDto dto, Images images)
    {
        _dto = dto;
        _images = images;
    }

    public Task Run(IServiceProvider serviceProvider, Window owner)
    {
        serviceProvider.GetRequiredService<IUI>().LaunchUrl(_dto.GetUrl());

        return Task.CompletedTask;
    }

    public (Guid TypeId, string Json) Serialize()
    {
        return (RepositoryFilePathType.Id, JsonSerializer.Serialize(_dto));
    }
}

internal record RepositoryFilePathMatchDto(RepositoryMatchDto Repository, string FilePath)
{
    public string GetUrl() => $"{Repository.GetUrl()}?path={Uri.EscapeDataString(FilePath)}";
};
