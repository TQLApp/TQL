﻿using Launcher.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Launcher.Plugins.AzureDevOps.Categories;

internal class BacklogMatch : IRunnableMatch, ISerializableMatch
{
    private readonly BacklogMatchDto _dto;
    private readonly Images _images;

    public string Text => $"{_dto.ProjectName}/{_dto.TeamName} {_dto.BacklogName} Backlog";
    public IImage Icon => _images.Boards;

    public BacklogMatch(BacklogMatchDto dto, Images images)
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
        return (BacklogType.Id, JsonSerializer.Serialize(_dto));
    }
}

internal record BacklogMatchDto(string Url, string ProjectName, string TeamName, string BacklogName)
{
    public string GetUrl() =>
        $"{Url}/_boards/board/t/{Uri.EscapeDataString(TeamName)}/{Uri.EscapeDataString(BacklogName)}";
};
