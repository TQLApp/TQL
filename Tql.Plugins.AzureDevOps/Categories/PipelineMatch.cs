using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using Tql.Abstractions;
using Tql.Utilities;
using Path = System.IO.Path;

namespace Tql.Plugins.AzureDevOps.Categories;

internal class PipelineMatch(PipelineMatchDto dto)
    : IRunnableMatch,
        ISerializableMatch,
        ICopyableMatch
{
    public string Text =>
        MatchText.Path(
            dto.ProjectName,
            Path.Combine(dto.Path.Trim('\\'), dto.Name).Replace('\\', '/')
        );

    public ImageSource Icon => Images.Pipelines;
    public MatchTypeId TypeId => TypeIds.Pipeline;

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

internal record PipelineMatchDto(string Url, string ProjectName, int Id, string Path, string Name)
{
    public string GetUrl() =>
        $"{Url.TrimEnd('/')}/{Uri.EscapeDataString(ProjectName)}/_build?definitionId={Id}";
};
