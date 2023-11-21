using Tql.Abstractions;
using Tql.Plugins.GitHub.Services;
using Tql.Plugins.GitHub.Support;
using Tql.Utilities;

namespace Tql.Plugins.GitHub.Categories;

[RootMatchType(SupportsUserScope = true)]
internal class RepositoriesType(
    IMatchFactory<RepositoriesMatch, RootItemDto> factory,
    ConfigurationManager configurationManager
) : MatchType<RepositoriesMatch, RootItemDto>(factory)
{
    public override Guid Id => TypeIds.Repositories.Id;

    protected override bool IsValid(RootItemDto dto) =>
        configurationManager.Configuration.HasConnection(dto.Id);
}
