using System.Text.RegularExpressions;
using Tql.Abstractions;
using Tql.Plugins.Jira.Services;
using Tql.Plugins.Jira.Support;

namespace Tql.Plugins.Jira.Categories;

internal class IssuesMatch : ISearchableMatch, ISerializableMatch
{
    // From https://confluence.atlassian.com/adminjiraserver/changing-the-project-key-format-938847081.html
    // We ignore case to simplify the user experience.
    private static readonly Regex KeyRe =
        new(@"^[A-Z][A-Z]+-\d+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private readonly string _url;
    private readonly JiraApi _api;
    private readonly IconCacheManager _iconCacheManager;

    public string Text { get; }
    public ImageSource Icon => Images.Issues;
    public MatchTypeId TypeId => TypeIds.Issues;

    public IssuesMatch(string text, string url, JiraApi api, IconCacheManager iconCacheManager)
    {
        _url = url;
        _api = api;
        _iconCacheManager = iconCacheManager;

        Text = text;
    }

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

        var client = _api.GetClient(_url);

        if (maybeKey)
        {
            try
            {
                var issue = await client.GetIssue(text.ToUpper(), cancellationToken);

                return await CreateMatches(new[] { issue }, client);
            }
            catch
            {
                // Ignore. Fall back to a normal search.
            }
        }

        var issues = await client.GetIssues(
            $"text ~ \"{text.Replace("\"", "\\\"")}*\"",
            100,
            cancellationToken
        );

        return await CreateMatches(issues, client);
    }

    private async Task<IEnumerable<IMatch>> CreateMatches(
        IEnumerable<JiraIssueDto> issues,
        JiraClient client
    )
    {
        var result = issues
            .Select(
                p => new IssueMatchDto(_url, p.Key, p.Fields.Summary, p.Fields.IssueType.IconUrl)
            )
            .ToList();

        // Seed the icon cache.

        foreach (var icon in result.Select(p => p.IssueTypeIconUrl).Distinct())
        {
            await _iconCacheManager.LoadIcon(client, icon);
        }

        return result.Select(p => new IssueMatch(p, _iconCacheManager));
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(new RootItemDto(_url));
    }
}
