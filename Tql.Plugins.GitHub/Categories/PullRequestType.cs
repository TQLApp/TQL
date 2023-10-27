using Tql.Abstractions;
using Tql.Plugins.GitHub.Services;
using Tql.Utilities;

namespace Tql.Plugins.GitHub.Categories;

internal class PullRequestType : IMatchType
{
    private readonly ConfigurationManager _configurationManager;

    public Guid Id => TypeIds.PullRequest.Id;

    public PullRequestType(ConfigurationManager configurationManager)
    {
        _configurationManager = configurationManager;
    }

    public IMatch? Deserialize(string json)
    {
        var dto = JsonSerializer.Deserialize<IssueMatchDto>(json)!;
        if (!_configurationManager.Configuration.HasConnection(dto.ConnectionId))
            return null;

        return new IssueMatch(dto);
    }
}
