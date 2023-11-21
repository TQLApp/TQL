using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using Tql.Abstractions;
using Tql.Plugins.Jira.Services;
using Tql.Utilities;

namespace Tql.Plugins.Jira.Categories;

internal class ProjectMatch(
    ProjectMatchDto dto,
    IconCacheManager iconCacheManager,
    ConfigurationManager configurationManager,
    IMatchFactory<IssueMatch, IssueMatchDto> factory
) : IRunnableMatch, ISerializableMatch, ICopyableMatch, ISearchableMatch
{
    public string Text => string.Format(Labels.ProjectMatch_Label, dto.Name);
    public ImageSource Icon { get; } =
        iconCacheManager.GetIcon(new IconKey(dto.Url, dto.AvatarUrl)) ?? Images.Projects;
    public MatchTypeId TypeId => TypeIds.Project;
    public string SearchHint => Labels.ProjectMatch_SearchHint;

    public Task Run(IServiceProvider serviceProvider, IWin32Window owner)
    {
        serviceProvider.GetRequiredService<IUI>().OpenUrl(dto.GetUrl());

        return Task.CompletedTask;
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(dto);
    }

    public Task Copy(IServiceProvider serviceProvider)
    {
        serviceProvider.GetRequiredService<IClipboard>().CopyUri(Text, dto.GetUrl());

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

        var client = configurationManager.GetClient(dto.Url);

        var issues = await client.SearchIssues(
            $"project = \"{dto.Key}\" and text ~ \"{text.Replace("\"", "\\\"")}*\"",
            100,
            cancellationToken
        );

        return IssueUtils.CreateMatches(dto.Url, issues, factory);
    }
}

internal record ProjectMatchDto(string Url, string Key, string Name, string AvatarUrl)
{
    public string GetUrl() => $"{Url.TrimEnd('/')}/browse/{Uri.EscapeDataString(Key)}";
};
