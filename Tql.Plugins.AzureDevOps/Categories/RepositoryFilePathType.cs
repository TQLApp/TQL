using Tql.Abstractions;
using Tql.Plugins.AzureDevOps.Services;
using Tql.Utilities;

namespace Tql.Plugins.AzureDevOps.Categories;

internal class RepositoryFilePathType(
    IMatchFactory<RepositoryFilePathMatch, RepositoryFilePathMatchDto> factory,
    ConfigurationManager configurationManager
) : MatchType<RepositoryFilePathMatch, RepositoryFilePathMatchDto>(factory)
{
    public override Guid Id => TypeIds.RepositoryFilePath.Id;

    protected override bool IsValid(RepositoryFilePathMatchDto dto) =>
        configurationManager.Configuration.HasConnection(dto.Repository.Url);
}
