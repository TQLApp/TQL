using Tql.Abstractions;
using Tql.Utilities;

namespace Tql.Plugins.AzureDevOps.Categories;

internal class PipelineType : IMatchType
{
    public Guid Id => TypeIds.Pipeline.Id;

    public IMatch Deserialize(string json)
    {
        return new PipelineMatch(JsonSerializer.Deserialize<PipelineMatchDto>(json)!);
    }
}
