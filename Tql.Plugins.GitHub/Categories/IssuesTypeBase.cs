using Tql.Abstractions;
using Tql.Plugins.GitHub.Services;
using Tql.Utilities;

namespace Tql.Plugins.GitHub.Categories;

internal abstract class IssuesTypeBase<TCategory, TMatch> : MatchType<TCategory, RootItemDto>
    where TCategory : IssuesMatchBase<TMatch>
    where TMatch : IssueMatchBase
{
    private readonly ConfigurationManager _configurationManager;

    protected IssuesTypeBase(
        IMatchFactory<TCategory, RootItemDto> factory,
        ConfigurationManager configurationManager
    )
        : base(factory)
    {
        _configurationManager = configurationManager;
    }

    protected override bool IsValid(RootItemDto dto) =>
        _configurationManager.Configuration.HasConnection(dto.Id);
}
