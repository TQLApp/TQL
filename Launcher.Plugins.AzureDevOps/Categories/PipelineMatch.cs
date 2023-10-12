using Launcher.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Launcher.Plugins.AzureDevOps.Categories;

internal class PipelineMatch : IRunnableMatch, ISerializableMatch, ICopyableMatch
{
    private readonly PipelineMatchDto _dto;

    public string Text =>
        System.IO.Path
            .Combine(_dto.ProjectName, _dto.Path.Trim('\\'), _dto.Name)
            .Replace('\\', '/');
    public ImageSource Icon => Images.Pipelines;
    public MatchTypeId TypeId => TypeIds.Pipeline;

    public PipelineMatch(PipelineMatchDto dto)
    {
        _dto = dto;
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

    public Task Copy(IServiceProvider serviceProvider)
    {
        serviceProvider.GetRequiredService<IClipboard>().CopyUri(Text, _dto.GetUrl());

        return Task.CompletedTask;
    }
}

internal record PipelineMatchDto(string Url, string ProjectName, int Id, string Path, string Name)
{
    public string GetUrl() =>
        $"{Url.TrimEnd('/')}/{Uri.EscapeDataString(ProjectName)}/_build?definitionId={Id}";
};
