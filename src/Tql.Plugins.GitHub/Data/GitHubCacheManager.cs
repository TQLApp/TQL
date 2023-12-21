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

    public int Version => 4;

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

            var repositories = await GetRepositories(
                user,
                organizations,
                client,
                graphQlConnection
            );

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
        GitHubClient client,
        GraphQLConnection graphQlConnection
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

            foreach (var repository in response.Items)
            {
                var issueTemplates = await GetIssueTemplates(
                    repository.Owner.Login,
                    repository.Name,
                    graphQlConnection
                );

                repositories.Add(
                    new GitHubRepository(
                        repository.Owner.Login,
                        repository.Name,
                        repository.HtmlUrl,
                        repository.UpdatedAt,
                        issueTemplates
                    )
                );
            }

            if (response.Items.Count == 0)
                break;
        }

        return repositories.ToImmutable();
    }

    private static async Task<ImmutableArray<GitHubIssueTemplate>> GetIssueTemplates(
        string owner,
        string name,
        GraphQLConnection graphQlConnection
    )
    {
        var query = new Query()
            .Repository(name, owner)
            .IssueTemplates.Select(p => new { p.Name, p.Filename })
            .Compile();

        return (
            from issueTemplate in await graphQlConnection.Run(query)
            select new GitHubIssueTemplate(issueTemplate.Name, issueTemplate.Filename)
        ).ToImmutableArray();
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
