using Octokit;
using Octokit.GraphQL;
using Octokit.GraphQL.Model;
using Tql.Abstractions;
using Tql.Plugins.GitHub.Services;
using Tql.Plugins.GitHub.Support;
using GraphQLConnection = Octokit.GraphQL.Connection;
using User = Octokit.User;

namespace Tql.Plugins.GitHub.Data;

internal class GitHubCacheManager : ICacheManager<GitHubData>
{
    private readonly ConfigurationManager _configurationManager;
    private readonly GitHubApi _api;

    public int Version => 3;

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
            var graphQlConnection = await _api.GetConnection(connection.Id);

            var user = await client.User.Current();
            var organizations = (
                from organization in await client.Organization.GetAllForCurrent()
                select organization.Login
            ).ToImmutableArray();

            var repositories = await GetRepositories(user, organizations, client);

            var projects = await GetProjects(user, organizations, graphQlConnection);

            connections.Add(
                new GitHubConnectionData(
                    connection.Id,
                    user.Login,
                    organizations,
                    repositories,
                    projects
                )
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
                response.Items.Select(
                    p => new GitHubRepository(p.Owner.Login, p.Name, p.HtmlUrl, p.UpdatedAt)
                )
            );

            if (response.Items.Count == 0)
                break;
        }

        return repositories.ToImmutable();
    }

    private async Task<ImmutableArray<GitHubProject>> GetProjects(
        User user,
        ImmutableArray<string> organizations,
        GraphQLConnection graphQlConnection
    )
    {
        var result = ImmutableArray.CreateBuilder<GitHubProject>();

        result.AddRange(await QueryProjects(user.Login, new Query().User(user.Login).ProjectsV2()));

        foreach (var organization in organizations)
        {
            result.AddRange(
                await QueryProjects(
                    organization,
                    new Query().Organization(organization).ProjectsV2()
                )
            );
        }

        return result.ToImmutable();

        async Task<IEnumerable<GitHubProject>> QueryProjects(
            string owner,
            ProjectV2Connection projectsQuery
        )
        {
            var query = projectsQuery
                .AllPages()
                .Select(
                    p =>
                        new
                        {
                            p.Number,
                            p.Title,
                            p.Url,
                            p.UpdatedAt
                        }
                );

            return from project in await graphQlConnection.Run(query)
                select new GitHubProject(
                    owner,
                    project.Number,
                    project.Title,
                    project.Url,
                    project.UpdatedAt
                );
        }
    }

    protected virtual void OnCacheInvalidationRequired(CacheInvalidationRequiredEventArgs e) =>
        CacheInvalidationRequired?.Invoke(this, e);
}
