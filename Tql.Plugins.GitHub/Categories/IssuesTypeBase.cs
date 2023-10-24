using Octokit;
using Tql.Abstractions;
using Tql.Plugins.GitHub.Data;
using Tql.Plugins.GitHub.Services;
using Tql.Plugins.GitHub.Support;
using Tql.Utilities;

namespace Tql.Plugins.GitHub.Categories;

internal abstract class IssuesTypeBase : IMatchType
{
    private readonly ConfigurationManager _configurationManager;
    private readonly GitHubApi _api;
    private readonly IssueTypeQualifier _type;
    private readonly ICache<GitHubData> _cache;

    public abstract Guid Id { get; }

    protected IssuesTypeBase(
        ConfigurationManager configurationManager,
        GitHubApi api,
        ICache<GitHubData> cache,
        IssueTypeQualifier type
    )
    {
        _configurationManager = configurationManager;
        _api = api;
        _cache = cache;
        _type = type;
    }

    public IMatch? Deserialize(string json)
    {
        var dto = JsonSerializer.Deserialize<RootItemDto>(json)!;
        var configuration = _configurationManager.Configuration;

        if (!configuration.Connections.Any(p => p.Id == dto.Id))
            return null;

        return CreateMatch(
            MatchUtils.GetMatchLabel(
                $"GitHub {(_type == IssueTypeQualifier.Issue ? "Issue" : "Pull Request")}",
                configuration,
                dto
            ),
            dto,
            _api,
            _cache
        );
    }

    protected abstract IssuesMatchBase CreateMatch(
        string text,
        RootItemDto dto,
        GitHubApi api,
        ICache<GitHubData> cache
    );
}
