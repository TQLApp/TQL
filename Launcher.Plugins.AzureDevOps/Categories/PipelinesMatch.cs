using Launcher.Abstractions;
using Launcher.Plugins.AzureDevOps.Data;
using Launcher.Utilities;

namespace Launcher.Plugins.AzureDevOps.Categories;

internal class PipelinesMatch : CachedMatch<AzureData>
{
    private readonly Images _images;
    private readonly string _url;

    public override string Text { get; }
    public override IImage Icon => _images.Pipelines;
    public override MatchTypeId TypeId => TypeIds.Pipelines;

    public PipelinesMatch(string text, Images images, string url, ICache<AzureData> cache)
        : base(cache)
    {
        _images = images;
        _url = url;

        Text = text;
    }

    protected override IEnumerable<IMatch> Create(AzureData data)
    {
        return from project in data.GetConnection(_url).Projects
            from buildDefinition in project.BuildDefinitions
            select new PipelineMatch(
                new PipelineMatchDto(
                    _url,
                    project.Name,
                    buildDefinition.Id,
                    buildDefinition.Path,
                    buildDefinition.Name
                ),
                _images
            );
    }
}
