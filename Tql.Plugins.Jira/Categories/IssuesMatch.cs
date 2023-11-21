using System.Text.RegularExpressions;
using Tql.Abstractions;
using Tql.Plugins.Jira.Services;
using Tql.Plugins.Jira.Support;
using Tql.Utilities;

namespace Tql.Plugins.Jira.Categories;

internal class IssuesMatch(
    RootItemDto dto,
    ConfigurationManager configurationManager,
    IMatchFactory<IssueMatch, IssueMatchDto> factory
) : ISearchableMatch, ISerializableMatch
{
    // From https://confluence.atlassian.com/adminjiraserver/changing-the-project-key-format-938847081.html
    // We ignore case to simplify the user experience.
    private static readonly Regex KeyRe =
        new(@"^[A-Z][A-Z]+-\d+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

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

        var maybeKey = KeyRe.IsMatch(text);
        if (maybeKey)
            context.SuppressPreliminaryResults();

        await context.DebounceDelay(cancellationToken);

        var client = configurationManager.GetClient(dto.Url);

        if (maybeKey)
        {
            try
            {
                var issue = await client.GetIssue(text.ToUpper(), cancellationToken);

                return IssueUtils.CreateMatches(dto.Url, new[] { issue }, factory);
            }
            catch
            {
                // Ignore. Fall back to a normal search.
            }
        }

        var issues = await client.SearchIssues(
            $"text ~ \"{text.Replace("\"", "\\\"")}*\"",
            100,
            cancellationToken
        );

        return IssueUtils.CreateMatches(dto.Url, issues, factory);
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(dto);
    }
}
