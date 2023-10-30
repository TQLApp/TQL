using Microsoft.Extensions.DependencyInjection;
using Tql.Abstractions;
using Tql.Plugins.Jira.Services;

namespace Tql.Plugins.Jira.Categories;

internal class NewMatch : IRunnableMatch, ISerializableMatch, ICopyableMatch
{
    private readonly NewMatchDto _dto;

    public string Text => $"{_dto.ProjectName} › {_dto.Name}";
    public ImageSource Icon { get; }
    public MatchTypeId TypeId => TypeIds.New;

    public NewMatch(NewMatchDto dto, IconCacheManager iconCacheManager)
    {
        _dto = dto;
        Icon = iconCacheManager.GetIcon(new IconKey(dto.Url, dto.IconUrl)) ?? Images.Issues;
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

internal record NewMatchDto(
    string Url,
    string ProjectName,
    string ProjectId,
    string Name,
    string Id,
    string IconUrl
)
{
    public string GetUrl() =>
        $"{Url.TrimEnd('/')}/secure/CreateIssue.jspa?issuetype={Uri.EscapeDataString(Id)}&pid={Uri.EscapeDataString(ProjectId)}";
};
