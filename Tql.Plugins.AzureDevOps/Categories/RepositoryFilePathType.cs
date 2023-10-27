using Tql.Abstractions;
using Tql.Plugins.AzureDevOps.Services;
using Tql.Utilities;

namespace Tql.Plugins.AzureDevOps.Categories;

internal class RepositoryFilePathType : IMatchType
{
    private readonly ConfigurationManager _configurationManager;

    public Guid Id => TypeIds.RepositoryFilePath.Id;

    public RepositoryFilePathType(ConfigurationManager configurationManager)
    {
        _configurationManager = configurationManager;
    }

    public IMatch? Deserialize(string json)
    {
        var dto = JsonSerializer.Deserialize<RepositoryFilePathMatchDto>(json)!;
        if (!_configurationManager.Configuration.HasConnection(dto.Repository.Url))
            return null;

        return new RepositoryFilePathMatch(dto);
    }
}
