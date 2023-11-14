using Tql.Abstractions;
using Tql.Plugins.GitHub.Services;
using Tql.Plugins.GitHub.Support;
using Tql.Utilities;

namespace Tql.Plugins.GitHub.Categories;

[RootMatchType(SupportsUserScope = true)]
internal class RepositoriesType : MatchType<RepositoriesMatch, RootItemDto>
{
    private readonly ConfigurationManager _configurationManager;

    public override Guid Id => TypeIds.Repositories.Id;

    public RepositoriesType(
        IMatchFactory<RepositoriesMatch, RootItemDto> factory,
        ConfigurationManager configurationManager
    )
        : base(factory)
    {
        _configurationManager = configurationManager;
    }

    protected override bool IsValid(RootItemDto dto) =>
        _configurationManager.Configuration.HasConnection(dto.Id);
}
