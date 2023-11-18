using Tql.Abstractions;
using Tql.Plugins.GitHub.Services;

namespace Tql.Plugins.GitHub.Data;

internal class GitHubCacheManager : ICacheManager<GitHubData>
{
    private readonly ConfigurationManager _configurationManager;
    private readonly GitHubApi _api;

    public int Version => 1;

    public event EventHandler<CacheExpiredEventArgs>? CacheExpired;

    public GitHubCacheManager(GitHubApi api, ConfigurationManager configurationManager)
    {
        _configurationManager = configurationManager;
        _api = api;

        configurationManager.Changed += (_, _) => OnCacheExpired(new CacheExpiredEventArgs(true));
    }

    public async Task<GitHubData> Create()
    {
        var connections = new List<GitHubConnectionData>();

        foreach (var connection in _configurationManager.Configuration.Connections)
        {
            var client = await _api.GetClient(connection.Id);

            var user = await client.User.Current();
            var organizations = await client.Organization.GetAllForCurrent();

            connections.Add(
                new GitHubConnectionData(
                    connection.Id,
                    user.Login,
                    organizations.Select(p => p.Login).ToImmutableArray()
                )
            );
        }

        return new GitHubData(connections.ToImmutableArray());
    }

    protected virtual void OnCacheExpired(CacheExpiredEventArgs e) => CacheExpired?.Invoke(this, e);
}
