using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using Tql.Abstractions;
using Tql.Plugins.Jira.Services;
using Tql.Utilities;

namespace Tql.Plugins.Jira.Categories;

internal class ProjectMatch : IRunnableMatch, ISerializableMatch, ICopyableMatch, ISearchableMatch
{
    private readonly ProjectMatchDto _dto;
    private readonly ConfigurationManager _configurationManager;
    private readonly IMatchFactory<IssueMatch, IssueMatchDto> _factory;

    public string Text => string.Format(Labels.ProjectMatch_Label, _dto.Name);
    public ImageSource Icon { get; }
    public MatchTypeId TypeId => TypeIds.Project;
    public string SearchHint => Labels.ProjectMatch_SearchHint;

    public ProjectMatch(
        ProjectMatchDto dto,
        IconCacheManager iconCacheManager,
        ConfigurationManager configurationManager,
        IMatchFactory<IssueMatch, IssueMatchDto> factory
    )
    {
        _dto = dto;
        _configurationManager = configurationManager;
        _factory = factory;

        Icon = iconCacheManager.GetIcon(new IconKey(dto.Url, dto.AvatarUrl)) ?? Images.Projects;
    }

    public Task Run(IServiceProvider serviceProvider, IWin32Window owner)
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

        var client = _configurationManager.GetClient(_dto.Url);

        var issues = await client.SearchIssues(
            $"project = \"{_dto.Key}\" and text ~ \"{text.Replace("\"", "\\\"")}*\"",
            100,
            cancellationToken
        );

        return IssueUtils.CreateMatches(_dto.Url, issues, _factory);
    }
}

internal record ProjectMatchDto(string Url, string Key, string Name, string AvatarUrl)
{
    public string GetUrl() => $"{Url.TrimEnd('/')}/browse/{Uri.EscapeDataString(Key)}";
};
