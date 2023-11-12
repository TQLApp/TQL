using Octokit;
using Tql.Abstractions;
using Tql.Plugins.GitHub.Data;
using Tql.Plugins.GitHub.Services;
using Tql.Plugins.GitHub.Support;
using Tql.Utilities;

namespace Tql.Plugins.GitHub.Categories;

internal class RepositoriesMatch : ISearchableMatch, ISerializableMatch
{
    private readonly RootItemDto _dto;
    private readonly GitHubApi _api;
    private readonly ICache<GitHubData> _cache;

    public string Text { get; }
    public ImageSource Icon => Images.Repository;
    public MatchTypeId TypeId => TypeIds.Repositories;

    public RepositoriesMatch(string text, RootItemDto dto, GitHubApi api, ICache<GitHubData> cache)
    {
        _dto = dto;
        _api = api;
        _cache = cache;

        Text = text;
    }

    public async Task<IEnumerable<IMatch>> Search(
        ISearchContext context,
        string text,
        CancellationToken cancellationToken
    )
    {
        if (_dto.Scope == RootItemScope.Global && text.IsWhiteSpace())
            return Array.Empty<IMatch>();

        await context.DebounceDelay(cancellationToken);

        var client = await _api.GetClient(_dto.Id);

        var request = new SearchRepositoriesRequest(
            await GitHubUtils.GetSearchPrefix(_dto, _cache) + text
        );

        if (text.IsWhiteSpace())
        {
            request.SortField = RepoSearchSort.Updated;
            request.Order = SortDirection.Descending;
        }

        var response = await client.Search.SearchRepo(request);

        cancellationToken.ThrowIfCancellationRequested();

        return response.Items.Select(
            p => new RepositoryMatch(new RepositoryMatchDto(_dto.Id, p.FullName, p.HtmlUrl), _api)
        );
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(_dto);
    }
}
