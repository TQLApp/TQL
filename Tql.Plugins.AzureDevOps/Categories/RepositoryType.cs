using Tql.Abstractions;
using Tql.Plugins.AzureDevOps.Services;
using Tql.Utilities;

namespace Tql.Plugins.AzureDevOps.Categories;

internal class RepositoryType : MatchType<RepositoryMatch, RepositoryMatchDto>
{
    private readonly ConfigurationManager _configurationManager;

    public override Guid Id => TypeIds.Repository.Id;

    public RepositoryType(
        IMatchFactory<RepositoryMatch, RepositoryMatchDto> factory,
        ConfigurationManager configurationManager
    )
        : base(factory)
    {
        _configurationManager = configurationManager;
    }

    protected override bool IsValid(RepositoryMatchDto dto) =>
        _configurationManager.Configuration.HasConnection(dto.Url);
}
