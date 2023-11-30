using Tql.Abstractions;
using Tql.Plugins.AzureDevOps.Services;
using Tql.Plugins.AzureDevOps.Support;
using Tql.Utilities;

namespace Tql.Plugins.AzureDevOps.Categories;

[RootMatchType]
internal class RepositoriesType(
    IMatchFactory<RepositoriesMatch, RootItemDto> factory,
    ConfigurationManager configurationManager
) : MatchType<RepositoriesMatch, RootItemDto>(factory)
{
    public override Guid Id => TypeIds.Repositories.Id;

    protected override bool IsValid(RootItemDto dto) =>
        configurationManager.Configuration.HasConnection(dto.Url);
}
