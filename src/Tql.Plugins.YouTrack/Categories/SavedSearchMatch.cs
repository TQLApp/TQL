using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using Tql.Abstractions;
using Tql.Plugins.YouTrack.Services;
using Tql.Plugins.YouTrack.Support;

namespace Tql.Plugins.YouTrack.Categories;

internal class SavedSearchMatch(
    SavedSearchMatchDto dto,
    ConfigurationManager configurationManager,
    IMatchFactory<IssueMatch, IssueMatchDto> factory
) : IRunnableMatch, ISerializableMatch, ICopyableMatch, ISearchableMatch
{
    public string Text => dto.Name;
    public ImageSource Icon => Images.SavedSearches;
    public MatchTypeId TypeId => TypeIds.SavedSearch;
    public string SearchHint => Labels.SavedSearchMatch_SearchHint;

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

        // The saved search narrowed with the text: "(<saved query>) and
        // (<text>)". A query that's still being typed is no results.
        try
        {
            var issues = await client.SearchIssues(
                YouTrackQueries.SavedSearch(dto.Query, text),
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

internal record SavedSearchMatchDto(string Url, string Id, string Name, string Query)
{
    public string GetUrl() => YouTrackUrls.GetSavedSearchUrl(Url, Query);
};
