using Tql.Abstractions;
using Tql.Plugins.Jira.Services;
using Tql.Plugins.Jira.Support;

namespace Tql.Plugins.Jira.Categories;

internal class IssuesMatch : ISearchableMatch, ISerializableMatch
{
    private readonly string _url;
    private readonly JiraApi _api;
    private readonly ConnectionManager _connectionManager;
    private readonly IconCacheManager _iconCacheManager;

    public string Text { get; }
    public ImageSource Icon => Images.Issues;
    public MatchTypeId TypeId => TypeIds.Issues;

    public IssuesMatch(
        string text,
        string url,
        JiraApi api,
        ConnectionManager connectionManager,
        IconCacheManager iconCacheManager
    )
    {
        _url = url;
        _api = api;
        _connectionManager = connectionManager;
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

        await context.DebounceDelay(cancellationToken);

        var connection = _connectionManager.Connections.Single(p => p.Url == _url);
        var client = _api.GetClient(connection);

        var issues = await client.GetIssues(
            $"text ~ \"{text.Replace("\"", "\\\"")}*\"",
            100,
            cancellationToken
        );

        var result = issues
            .Select(
                p =>
                    new IssueMatchDto(
                        _url,
                        p.Key,
                        p.Fields.Summary,
                        p.Fields.IssueType.Name,
                        p.Fields.IssueType.IconUrl
                    )
            )
            .ToList();

        // Seed the icon cache.

        foreach (var icon in result.Select(p => p.IssueTypeIconUrl).Distinct())
        {
            await _iconCacheManager.DownloadIcon(connection, icon);
        }

        return result.Select(p => new IssueMatch(p, _iconCacheManager, _connectionManager));
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(new RootItemDto(_url));
    }
}
