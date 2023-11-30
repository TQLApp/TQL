using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using Tql.Abstractions;
using Tql.Utilities;

namespace Tql.Plugins.AzureDevOps.Categories;

internal class RepositoryFilePathMatch(RepositoryFilePathMatchDto dto)
    : IRunnableMatch,
        ISerializableMatch,
        ICopyableMatch
{
    public string Text =>
        MatchText.Path(dto.Repository.ProjectName, dto.Repository.RepositoryName, dto.FilePath);

    public ImageSource Icon => Images.Document;
    public MatchTypeId TypeId => TypeIds.RepositoryFilePath;

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

internal record RepositoryFilePathMatchDto(RepositoryMatchDto Repository, string FilePath)
{
    public string GetUrl() => $"{Repository.GetUrl()}?path={Uri.EscapeDataString(FilePath)}";
};
