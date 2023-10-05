using Launcher.Abstractions;
using Launcher.Utilities;

namespace Launcher.Plugins.AzureDevOps.Categories;

internal class PipelineType : IMatchType
{
    private readonly Images _images;

    public Guid Id => TypeIds.Pipeline.Id;

    public PipelineType(Images images)
    {
        _images = images;
    }

    public IMatch Deserialize(string json)
    {
        return new PipelineMatch(JsonSerializer.Deserialize<PipelineMatchDto>(json)!, _images);
    }
}
