using Microsoft.Extensions.DependencyInjection;
using Tql.Abstractions;
using Tql.Plugins.Jira.Services;

namespace Tql.Plugins.Jira.Categories;

internal class ProjectMatch : IRunnableMatch, ISerializableMatch, ICopyableMatch
{
    private readonly ProjectMatchDto _dto;

    public string Text => $"{_dto.Name} Project";
    public ImageSource Icon { get; }
    public MatchTypeId TypeId => TypeIds.Project;

    public ProjectMatch(ProjectMatchDto dto, IconCacheManager iconCacheManager)
    {
        _dto = dto;
        Icon = iconCacheManager.GetIcon(dto.AvatarUrl) ?? Images.Projects;
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

internal record ProjectMatchDto(string Url, string Key, string Name, string AvatarUrl)
{
    public string GetUrl() => $"{Url.TrimEnd('/')}/browse/{Uri.EscapeDataString(Key)}";
};
