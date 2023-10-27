using Tql.Abstractions;
using Tql.Plugins.AzureDevOps.Services;
using Tql.Utilities;

namespace Tql.Plugins.AzureDevOps.Categories;

internal class PipelineType : IMatchType
{
    private readonly ConfigurationManager _configurationManager;

    public Guid Id => TypeIds.Pipeline.Id;

    public PipelineType(ConfigurationManager configurationManager)
    {
        _configurationManager = configurationManager;
    }

    public IMatch? Deserialize(string json)
    {
        var dto = JsonSerializer.Deserialize<PipelineMatchDto>(json)!;
        if (!_configurationManager.Configuration.HasConnection(dto.Url))
            return null;

        return new PipelineMatch(dto);
    }
}
