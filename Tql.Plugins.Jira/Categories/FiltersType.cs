using Tql.Abstractions;
using Tql.Plugins.Jira.Services;
using Tql.Plugins.Jira.Support;
using Tql.Utilities;

namespace Tql.Plugins.Jira.Categories;

[RootMatchType]
internal class FiltersType : MatchType<FiltersMatch, RootItemDto>
{
    private readonly ConfigurationManager _configurationManager;

    public override Guid Id => TypeIds.Filters.Id;

    public FiltersType(
        IMatchFactory<FiltersMatch, RootItemDto> factory,
        ConfigurationManager configurationManager
    )
        : base(factory)
    {
        _configurationManager = configurationManager;
    }

    protected override bool IsValid(RootItemDto dto) =>
        _configurationManager.Configuration.HasConnection(dto.Url);
}
