using Launcher.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Launcher.Plugins.AzureDevOps.Categories;

internal class PipelineMatch : IRunnableMatch, ISerializableMatch
{
    private readonly PipelineMatchDto _dto;
    private readonly Images _images;

    public string Text =>
        System.IO.Path
            .Combine(_dto.ProjectName, _dto.Path.Trim('\\'), _dto.Name)
            .Replace('\\', '/');
    public IImage Icon => _images.Pipelines;
    public MatchTypeId TypeId => TypeIds.Pipeline;

    public PipelineMatch(PipelineMatchDto dto, Images images)
    {
        _dto = dto;
        _images = images;
    }

    public Task Run(IServiceProvider serviceProvider, Window owner)
    {
        serviceProvider.GetRequiredService<IUI>().LaunchUrl(_dto.GetUrl());

        return Task.CompletedTask;
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(_dto);
    }
}

internal record PipelineMatchDto(string Url, string ProjectName, int Id, string Path, string Name)
{
    public string GetUrl() =>
        $"{Url.TrimEnd('/')}/{Uri.EscapeDataString(ProjectName)}/_build?definitionId={Id}";
};
