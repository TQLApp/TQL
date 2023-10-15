using Octokit;
using Tql.Abstractions;
using Tql.Plugins.GitHub.Services;
using Tql.Plugins.GitHub.Support;

namespace Tql.Plugins.GitHub.Categories;

internal class RepositoriesMatch : ISearchableMatch, ISerializableMatch
{
    private readonly Guid _id;
    private readonly GitHubApi _api;

    public string Text { get; }
    public ImageSource Icon => Images.Repository;
    public MatchTypeId TypeId => TypeIds.Repositories;

    public RepositoriesMatch(string text, Guid id, GitHubApi api)
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

        var response = await client.Search.SearchRepo(new SearchRepositoriesRequest(text));

        cancellationToken.ThrowIfCancellationRequested();

        return response.Items.Select(
            p => new RepositoryMatch(new RepositoryMatchDto(_id, p.FullName, p.HtmlUrl), _api)
        );
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(new RootItemDto(_id));
    }
}
