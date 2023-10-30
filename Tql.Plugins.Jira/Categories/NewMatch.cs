using Microsoft.Extensions.DependencyInjection;
using Tql.Abstractions;
using Tql.Plugins.Jira.Data;
using Tql.Plugins.Jira.Services;
using Tql.Plugins.Jira.Support;

namespace Tql.Plugins.Jira.Categories;

internal class NewMatch : IRunnableMatch, ISerializableMatch, ICopyableMatch
{
    private readonly NewMatchDto _dto;
    private readonly ICache<JiraData> _cache;

    public string Text => $"{_dto.ProjectName} › {_dto.Name}";
    public ImageSource Icon { get; }
    public MatchTypeId TypeId => TypeIds.New;

    public NewMatch(NewMatchDto dto, IconCacheManager iconCacheManager, ICache<JiraData> cache)
    {
        _dto = dto;
        _cache = cache;

        var icon = default(ImageSource);
        if (dto.IconUrl != null)
            icon = iconCacheManager.GetIcon(new IconKey(dto.Url, dto.IconUrl));
        Icon = icon ?? Images.Issues;
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
        if (_dto.Type == NewMatchType.Issue)
        {
            return $"{_dto.Url.TrimEnd('/')}/secure/CreateIssue.jspa"
                + $"?issuetype={Uri.EscapeDataString(_dto.Id!)}"
                + $"&pid={Uri.EscapeDataString(_dto.ProjectId)}";
        }

        var cache = await _cache.Get();
        var connection = cache.GetConnection(_dto.Url);
        var project = connection.Projects.Single(p => p.Id == _dto.ProjectId);

        var projectUrl = MatchUtils.GetProjectUrl(
            _dto.Url,
            project.Key,
            project.ProjectTypeKey,
            string.Equals(project.Style, "classic", StringComparison.OrdinalIgnoreCase)
        );

        return projectUrl + "/issues";
    }
}

internal record NewMatchDto(
    string Url,
    string ProjectName,
    string ProjectId,
    NewMatchType Type,
    string Name,
    string? Id,
    string? IconUrl
);

internal enum NewMatchType
{
    Query,
    Issue
}
