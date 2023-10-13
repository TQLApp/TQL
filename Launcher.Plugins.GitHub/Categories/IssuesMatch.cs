using Launcher.Abstractions;
using Launcher.Plugins.GitHub.Services;
using Launcher.Plugins.GitHub.Support;
using Octokit;

namespace Launcher.Plugins.GitHub.Categories;

internal class IssuesMatch : ISearchableMatch, ISerializableMatch
{
    private readonly Guid _id;
    private readonly GitHubApi _api;

    public string Text { get; }
    public ImageSource Icon => Images.Issue;
    public MatchTypeId TypeId => TypeIds.Issues;

    public IssuesMatch(string text, Guid id, GitHubApi api)
    {
        _id = id;
        _api = api;

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

        var client = await _api.GetClient(_id);

        var response = await client.Search.SearchIssues(new SearchIssuesRequest(text));

        cancellationToken.ThrowIfCancellationRequested();

        return response.Items.Select(
            p =>
                new IssueMatch(
                    new IssueMatchDto(
                        _id,
                        GitHubUtils.GetRepositoryName(p.HtmlUrl),
                        p.Number,
                        p.Title,
                        p.HtmlUrl,
                        p.State.Value
                    )
                )
        );
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(new RootItemDto(_id));
    }
}
