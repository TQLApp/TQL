using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using Tql.Abstractions;
using Tql.Plugins.YouTrack.Services;
using Tql.Plugins.YouTrack.Support;

namespace Tql.Plugins.YouTrack.Categories;

internal class ProjectMatch(
    ProjectMatchDto dto,
    IconCacheManager iconCacheManager,
    ConfigurationManager configurationManager,
    IMatchFactory<IssueMatch, IssueMatchDto> factory
) : IRunnableMatch, ISerializableMatch, ICopyableMatch, ISearchableMatch
{
    public string Text => string.Format(Labels.ProjectMatch_Label, dto.Name);
    public ImageSource Icon { get; } =
        (dto.IconUrl != null ? iconCacheManager.GetIcon(new IconKey(dto.Url, dto.IconUrl)) : null)
        ?? Images.Projects;
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
        await context.DebounceDelay(cancellationToken);

        var client = configurationManager.GetClient(dto.Url);

        // "project: KC <text>", or the most recently updated issues without
        // text. A query that's still being typed is no results.
        try
        {
            var issues = await client.SearchIssues(
                YouTrackQueries.Project(dto.ShortName, text),
                YouTrackClient.DefaultSearchTop,
                cancellationToken
            );

            return IssueUtils.CreateMatches(dto.Url, issues, factory);
        }
        catch (YouTrackInvalidQueryException)
        {
            return Array.Empty<IMatch>();
        }
    }
}

internal record ProjectMatchDto(string Url, string ShortName, string Name, string? IconUrl)
{
    public string GetUrl() => YouTrackUrls.GetProjectUrl(Url, ShortName);
};
