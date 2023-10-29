using Microsoft.Extensions.DependencyInjection;
using Tql.Abstractions;

namespace Tql.Plugins.AzureDevOps.Categories;

internal class BoardMatch : IRunnableMatch, ISerializableMatch, ICopyableMatch
{
    private readonly BoardMatchDto _dto;

    public string Text => $"{_dto.ProjectName} › {_dto.TeamName} {_dto.BoardName} Board";
    public ImageSource Icon => Images.Boards;
    public MatchTypeId TypeId => TypeIds.Board;

    public BoardMatch(BoardMatchDto dto)
    {
        _dto = dto;
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

internal record BoardMatchDto(string Url, string ProjectName, string TeamName, string BoardName)
{
    public string GetUrl() =>
        $"{Url.TrimEnd('/')}/{Uri.EscapeDataString(ProjectName)}/_boards/board/t/{Uri.EscapeDataString(TeamName)}/{Uri.EscapeDataString(BoardName)}";
};
