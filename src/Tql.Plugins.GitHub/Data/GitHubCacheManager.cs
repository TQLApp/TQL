using Octokit;
using Tql.Abstractions;
using Tql.Plugins.GitHub.Services;
using Tql.Plugins.GitHub.Support;

namespace Tql.Plugins.GitHub.Data;

internal class GitHubCacheManager : ICacheManager<GitHubData>
{
    private readonly ConfigurationManager _configurationManager;
    private readonly GitHubApi _api;

    public int Version => 2;

    public event EventHandler<CacheInvalidationRequiredEventArgs>? CacheInvalidationRequired;

    public GitHubCacheManager(GitHubApi api, ConfigurationManager configurationManager)
    {
        _configurationManager = configurationManager;
        _api = api;

        configurationManager.Changed += (_, _) =>
            OnCacheInvalidationRequired(new CacheInvalidationRequiredEventArgs(true));
    }

    public async Task<GitHubData> Create()
    {
        var connections = new List<GitHubConnectionData>();

        foreach (var connection in _configurationManager.Configuration.Connections)
        {
            var client = await _api.GetClient(connection.Id);

            var user = await client.User.Current();
            var organizations = (
                from organization in await client.Organization.GetAllForCurrent()
                select organization.Login
            ).ToImmutableArray();

            var repositories = await GetRepositories(user, organizations, client);

            connections.Add(
                new GitHubConnectionData(connection.Id, user.Login, organizations, repositories)
            );
        }

        return new GitHubData(connections.ToImmutableArray());
    }

    private static async Task<ImmutableArray<GitHubRepository>> GetRepositories(
        User user,
        ImmutableArray<string> organizations,
        GitHubClient client
    )
    {
        var repositories = ImmutableArray.CreateBuilder<GitHubRepository>();

        var request = new SearchRepositoriesRequest(
            GitHubUtils.GetSearchPrefix(user.Login, organizations)
        );

        for (var page = 1; ; page++)
        {
            request.Page = page;

            var response = await client.Search.SearchRepo(request);

            repositories.AddRange(
                response
                    .Items
                    .Select(
                        p => new GitHubRepository(p.Owner.Login, p.Name, p.HtmlUrl, p.UpdatedAt)
                    )
            );

            if (response.Items.Count == 0)
                break;
        }

        return repositories.ToImmutable();
    }

    protected virtual void OnCacheInvalidationRequired(CacheInvalidationRequiredEventArgs e) =>
        CacheInvalidationRequired?.Invoke(this, e);
}
