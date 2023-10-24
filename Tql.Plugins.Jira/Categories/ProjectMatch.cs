using Microsoft.Extensions.DependencyInjection;
using Tql.Abstractions;
using Tql.Plugins.Jira.Services;
using Tql.Plugins.Jira.Support;

namespace Tql.Plugins.Jira.Categories;

internal class ProjectMatch
    : IRunnableMatch,
        ISerializableMatch,
        ICopyableMatch,
        ISearchableMatch,
        IHasSearchHint
{
    private readonly ProjectMatchDto _dto;
    private readonly IconCacheManager _iconCacheManager;
    private readonly JiraApi _api;

    public string Text => $"{_dto.Name} Project";
    public ImageSource Icon { get; }
    public MatchTypeId TypeId => TypeIds.Project;
    public string SearchHint => "Find issues";

    public ProjectMatch(ProjectMatchDto dto, IconCacheManager iconCacheManager, JiraApi api)
    {
        _dto = dto;
        _iconCacheManager = iconCacheManager;
        _api = api;

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

    public async Task<IEnumerable<IMatch>> Search(
        ISearchContext context,
        string text,
        CancellationToken cancellationToken
    )
    {
        if (text.IsWhiteSpace())
            return Array.Empty<IMatch>();

        await context.DebounceDelay(cancellationToken);

        var client = _api.GetClient(_dto.Url);

        var issues = await client.GetIssues(
            $"project = \"{_dto.Key}\" and text ~ \"{text.Replace("\"", "\\\"")}*\"",
            100,
            cancellationToken
        );

        return await IssueUtils.CreateMatches(_dto.Url, issues, client, _iconCacheManager);
    }
}

internal record ProjectMatchDto(string Url, string Key, string Name, string AvatarUrl)
{
    public string GetUrl() => $"{Url.TrimEnd('/')}/browse/{Uri.EscapeDataString(Key)}";
};
