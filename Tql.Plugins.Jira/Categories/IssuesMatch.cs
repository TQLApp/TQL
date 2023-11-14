using System.Text.RegularExpressions;
using Tql.Abstractions;
using Tql.Plugins.Jira.Services;
using Tql.Plugins.Jira.Support;
using Tql.Utilities;

namespace Tql.Plugins.Jira.Categories;

internal class IssuesMatch : ISearchableMatch, ISerializableMatch
{
    // From https://confluence.atlassian.com/adminjiraserver/changing-the-project-key-format-938847081.html
    // We ignore case to simplify the user experience.
    private static readonly Regex KeyRe =
        new(@"^[A-Z][A-Z]+-\d+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private readonly RootItemDto _dto;
    private readonly ConfigurationManager _configurationManager;
    private readonly IMatchFactory<IssueMatch, IssueMatchDto> _factory;

    public string Text =>
        MatchUtils.GetMatchLabel(
            Labels.IssuesMatch_Label,
            _configurationManager.Configuration,
            _dto.Url
        );

    public ImageSource Icon => Images.Issues;
    public MatchTypeId TypeId => TypeIds.Issues;
    public string SearchHint => Labels.IssuesMatch_SearchHint;

    public IssuesMatch(
        RootItemDto dto,
        ConfigurationManager configurationManager,
        IMatchFactory<IssueMatch, IssueMatchDto> factory
    )
    {
        _dto = dto;
        _configurationManager = configurationManager;
        _factory = factory;
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

        var client = _configurationManager.GetClient(_dto.Url);

        if (maybeKey)
        {
            try
            {
                var issue = await client.GetIssue(text.ToUpper(), cancellationToken);

                return IssueUtils.CreateMatches(_dto.Url, new[] { issue }, _factory);
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

        return IssueUtils.CreateMatches(_dto.Url, issues, _factory);
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(_dto);
    }
}
