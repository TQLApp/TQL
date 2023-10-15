using Microsoft.Extensions.DependencyInjection;
using Tql.Abstractions;

namespace Tql.Plugins.AzureDevOps.Categories;

internal class BacklogMatch : IRunnableMatch, ISerializableMatch, ICopyableMatch
{
    private readonly BacklogMatchDto _dto;

    public string Text => $"{_dto.ProjectName}/{_dto.TeamName} {_dto.BacklogName} Backlog";
    public ImageSource Icon => Images.Boards;
    public MatchTypeId TypeId => TypeIds.Backlog;

    public BacklogMatch(BacklogMatchDto dto)
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

internal record BacklogMatchDto(string Url, string ProjectName, string TeamName, string BacklogName)
{
    public string GetUrl() =>
        $"{Url.TrimEnd('/')}/{Uri.EscapeDataString(ProjectName)}/_backlogs/backlog/{Uri.EscapeDataString(TeamName)}/{Uri.EscapeDataString(BacklogName)}";
};
