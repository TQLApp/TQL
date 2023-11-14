using Tql.Abstractions;
using Tql.Plugins.AzureDevOps.Services;
using Tql.Utilities;

namespace Tql.Plugins.AzureDevOps.Categories;

internal class RepositoryFilePathType
    : MatchType<RepositoryFilePathMatch, RepositoryFilePathMatchDto>
{
    private readonly ConfigurationManager _configurationManager;

    public override Guid Id => TypeIds.RepositoryFilePath.Id;

    public RepositoryFilePathType(
        IMatchFactory<RepositoryFilePathMatch, RepositoryFilePathMatchDto> factory,
        ConfigurationManager configurationManager
    )
        : base(factory)
    {
        _configurationManager = configurationManager;
    }

    protected override bool IsValid(RepositoryFilePathMatchDto dto) =>
        _configurationManager.Configuration.HasConnection(dto.Repository.Url);
}
