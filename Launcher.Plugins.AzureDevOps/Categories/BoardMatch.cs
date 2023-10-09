﻿using Launcher.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Launcher.Plugins.AzureDevOps.Categories;

internal class BoardMatch : IRunnableMatch, ISerializableMatch
{
    private readonly BoardMatchDto _dto;
    private readonly Images _images;

    public string Text => $"{_dto.ProjectName}/{_dto.TeamName} {_dto.BoardName} Board";
    public IImage Icon => _images.Boards;
    public MatchTypeId TypeId => TypeIds.Board;

    public BoardMatch(BoardMatchDto dto, Images images)
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

internal record BoardMatchDto(string Url, string ProjectName, string TeamName, string BoardName)
{
    public string GetUrl() =>
        $"{Url.TrimEnd('/')}/{Uri.EscapeDataString(ProjectName)}/_boards/board/t/{Uri.EscapeDataString(TeamName)}/{Uri.EscapeDataString(BoardName)}";
};