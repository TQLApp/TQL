using Octokit;
using Tql.Abstractions;
using Tql.Plugins.GitHub.Services;
using Tql.Plugins.GitHub.Support;
using Tql.Utilities;

namespace Tql.Plugins.GitHub.Categories;

internal abstract class IssuesTypeBase : IMatchType
{
    private readonly ConnectionManager _connectionManager;
    private readonly GitHubApi _api;
    private readonly IssueTypeQualifier _type;

    public abstract Guid Id { get; }

    protected IssuesTypeBase(
        ConnectionManager connectionManager,
        GitHubApi api,
        IssueTypeQualifier type
    )
    {
        _connectionManager = connectionManager;
        _api = api;
        _type = type;
    }

    public IMatch? Deserialize(string json)
    {
        var dto = JsonSerializer.Deserialize<RootItemDto>(json)!;

        if (!_connectionManager.Connections.Any(p => p.Id == dto.Id))
            return null;

        return CreateMatch(
            MatchUtils.GetMatchLabel(
                $"GitHub {(_type == IssueTypeQualifier.Issue ? "Issue" : "Pull Request")}",
                _connectionManager,
                dto.Id
            ),
            dto.Id,
            _api
        );
    }

    protected abstract IssuesMatchBase CreateMatch(string text, Guid id, GitHubApi api);
}
