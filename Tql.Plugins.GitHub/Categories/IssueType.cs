using Tql.Abstractions;
using Tql.Plugins.GitHub.Services;
using Tql.Utilities;

namespace Tql.Plugins.GitHub.Categories;

internal class IssueType : MatchType<IssueMatch, IssueMatchDto>
{
    private readonly ConfigurationManager _configurationManager;

    public override Guid Id => TypeIds.Issue.Id;

    public IssueType(
        IMatchFactory<IssueMatch, IssueMatchDto> factory,
        ConfigurationManager configurationManager
    )
        : base(factory)
    {
        _configurationManager = configurationManager;
    }

    protected override bool IsValid(IssueMatchDto dto) =>
        _configurationManager.Configuration.HasConnection(dto.ConnectionId);
}
