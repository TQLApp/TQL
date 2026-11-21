using Tql.Abstractions;
using Tql.Plugins.YouTrack.Services;
using Tql.Plugins.YouTrack.Support;
using Tql.Utilities;

namespace Tql.Plugins.YouTrack.Categories;

internal class IssuesMatch(
    RootItemDto dto,
    ConfigurationManager configurationManager,
    IMatchFactory<IssueMatch, IssueMatchDto> factory
) : ISearchableMatch, ISerializableMatch
{
    public string Text =>
        MatchUtils.GetMatchLabel(
            Labels.IssuesMatch_Label,
            configurationManager.Configuration,
            dto.Url
        );

    public ImageSource Icon => Images.Issues;
    public MatchTypeId TypeId => TypeIds.Issues;
    public string SearchHint => Labels.IssuesMatch_SearchHint;

    public async Task<IEnumerable<IMatch>> Search(
        ISearchContext context,
        string text,
        CancellationToken cancellationToken
    )
    {
        if (text.IsWhiteSpace())
            return Array.Empty<IMatch>();

        var maybeId = YouTrackQueries.IsIssueId(text);
        if (maybeId)
            context.SuppressPreliminaryResults();

        await context.DebounceDelay(cancellationToken);

        var client = configurationManager.GetClient(dto.Url);

        if (maybeId)
        {
            try
            {
                var issue = await client.GetIssue(text, cancellationToken);
                if (issue != null)
                    return IssueUtils.CreateMatches(dto.Url, new[] { issue }, factory);
            }
            catch (YouTrackException)
            {
                // Ignore. Fall back to a normal search.
            }
        }

        // The text goes to YouTrack as a query, so "#Unresolved for: me"
        // works. A query that's still being typed, such as "#Unres", doesn't
        // parse; that's no results rather than an error.
        try
        {
            var issues = await client.SearchIssues(
                text.Trim(),
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

    public string Serialize()
    {
        return JsonSerializer.Serialize(dto);
    }
}
