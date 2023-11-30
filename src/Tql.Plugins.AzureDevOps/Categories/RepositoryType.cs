using Tql.Abstractions;
using Tql.Plugins.AzureDevOps.Services;
using Tql.Utilities;

namespace Tql.Plugins.AzureDevOps.Categories;

internal class RepositoryType(
    IMatchFactory<RepositoryMatch, RepositoryMatchDto> factory,
    ConfigurationManager configurationManager
) : MatchType<RepositoryMatch, RepositoryMatchDto>(factory)
{
    public override Guid Id => TypeIds.Repository.Id;

    protected override bool IsValid(RepositoryMatchDto dto) =>
        configurationManager.Configuration.HasConnection(dto.Url);
}
