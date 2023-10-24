using Tql.Abstractions;
using Tql.Plugins.GitHub.Services;

namespace Tql.Plugins.GitHub.Data;

internal class GitHubCacheManager : ICacheManager<GitHubData>
{
    private readonly IConfigurationManager _configurationManager;
    private readonly GitHubApi _api;

    public int Version => 1;

    public event EventHandler<CacheExpiredEventArgs>? CacheExpired;

    public GitHubCacheManager(
        IConfigurationManager configurationManager,
        GitHubApi api,
        ConfigurationManager configurationManagerImpl
    )
    {
        _configurationManager = configurationManager;
        _api = api;

        configurationManagerImpl.Changed += (_, _) =>
            OnCacheExpired(new CacheExpiredEventArgs(true));
    }

    public async Task<GitHubData> Create()
    {
        var configuration = Configuration.FromJson(
            _configurationManager.GetConfiguration(GitHubPlugin.Id)
        );

        var connections = new List<GitHubConnectionData>();

        foreach (var connection in configuration.Connections)
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
