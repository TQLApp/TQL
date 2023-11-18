using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using Tql.Abstractions;
using Tql.Utilities;

namespace Tql.Plugins.AzureDevOps.Categories;

internal class RepositoryFilePathMatch : IRunnableMatch, ISerializableMatch, ICopyableMatch
{
    private readonly RepositoryFilePathMatchDto _dto;

    public string Text =>
        MatchText.Path(_dto.Repository.ProjectName, _dto.Repository.RepositoryName, _dto.FilePath);

    public ImageSource Icon => Images.Document;
    public MatchTypeId TypeId => TypeIds.RepositoryFilePath;

    public RepositoryFilePathMatch(RepositoryFilePathMatchDto dto)
    {
        _dto = dto;
    }

    public Task Run(IServiceProvider serviceProvider, IWin32Window owner)
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

internal record RepositoryFilePathMatchDto(RepositoryMatchDto Repository, string FilePath)
{
    public string GetUrl() => $"{Repository.GetUrl()}?path={Uri.EscapeDataString(FilePath)}";
};
