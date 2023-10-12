using Launcher.Abstractions;
using Launcher.Utilities;

namespace Launcher.Plugins.AzureDevOps.Categories;

internal class PipelineType : IMatchType
{
    public Guid Id => TypeIds.Pipeline.Id;

    public IMatch Deserialize(string json)
    {
        return new PipelineMatch(JsonSerializer.Deserialize<PipelineMatchDto>(json)!);
    }
}
