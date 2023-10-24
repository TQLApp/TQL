using Microsoft.Extensions.DependencyInjection;
using Tql.Abstractions;
using Tql.Plugins.Jira.Services;

namespace Tql.Plugins.Jira.Categories;

internal class BoardMatch : IRunnableMatch, ISerializableMatch, ICopyableMatch
{
    private readonly BoardMatchDto _dto;

    public string Text => $"{_dto.Name} Board";
    public ImageSource Icon { get; }
    public MatchTypeId TypeId => TypeIds.Board;

    public BoardMatch(BoardMatchDto dto, IconCacheManager iconCacheManager)
    {
        _dto = dto;
        Icon = iconCacheManager.GetIcon(dto.AvatarUrl) ?? Images.Boards;
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

internal record BoardMatchDto(
    string Url,
    int Id,
    string Name,
    string ProjectKey,
    string ProjectTypeKey,
    string AvatarUrl
)
{
    public string GetUrl() =>
        $"{Url.TrimEnd('/')}/jira/{Uri.EscapeDataString(ProjectTypeKey)}/projects/{Uri.EscapeDataString(ProjectKey)}/boards/{Id}";
};
