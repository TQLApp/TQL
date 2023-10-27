using Microsoft.Extensions.DependencyInjection;
using Tql.Abstractions;
using Tql.Plugins.Jira.Data;
using Tql.Plugins.Jira.Services;

namespace Tql.Plugins.Jira.Categories;

internal class BoardMatch : IRunnableMatch, ISerializableMatch, ICopyableMatch
{
    private readonly BoardMatchDto _dto;
    private readonly ICache<JiraData> _cache;

    public string Text => $"{_dto.Name} - {_dto.MatchType}";
    public ImageSource Icon { get; }
    public MatchTypeId TypeId => TypeIds.Board;

    public BoardMatch(BoardMatchDto dto, IconCacheManager iconCacheManager, ICache<JiraData> cache)
    {
        _dto = dto;
        _cache = cache;

        Icon = iconCacheManager.GetIcon(dto.AvatarUrl) ?? Images.Boards;
    }

    public async Task Run(IServiceProvider serviceProvider, Window owner)
    {
        serviceProvider.GetRequiredService<IUI>().OpenUrl(await GetUrl());
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(_dto);
    }

    public async Task Copy(IServiceProvider serviceProvider)
    {
        serviceProvider.GetRequiredService<IClipboard>().CopyUri(Text, await GetUrl());
    }

    private async Task<string> GetUrl()
    {
        var cache = await _cache.Get();

        var project = cache
            .GetConnection(_dto.Url)
            .Projects.Single(
                p => string.Equals(p.Key, _dto.ProjectKey, StringComparison.OrdinalIgnoreCase)
            );

        var url = $"{_dto.Url.TrimEnd('/')}/jira/{Uri.EscapeDataString(_dto.ProjectTypeKey)}";
        if (string.Equals(project.Style, "classic", StringComparison.OrdinalIgnoreCase))
            url += "/c";
        url += $"/projects/{Uri.EscapeDataString(_dto.ProjectKey)}/boards/{_dto.Id}";

        switch (_dto.MatchType)
        {
            case BoardMatchType.Backlog:
                url += "/backlog";
                break;
            case BoardMatchType.Timeline:
                url += "/timeline";
                break;
        }

        return url;
    }
}

internal record BoardMatchDto(
    string Url,
    int Id,
    string Name,
    string ProjectKey,
    string ProjectTypeKey,
    string AvatarUrl,
    BoardMatchType MatchType
);

internal enum BoardMatchType
{
    Backlog,
    Board,
    Timeline
}
