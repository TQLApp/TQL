using Launcher.Abstractions;
using Launcher.Plugins.AzureDevOps.Services;

namespace Launcher.Plugins.AzureDevOps.Categories.People;

internal class TeamsVideosMatch : GraphUserMatchBase
{
    private readonly Images _images;

    public override string Text { get; }
    public override IImage Icon => _images.Teams;
    public override MatchTypeId TypeId => TypeIds.TeamsVideos;

    public TeamsVideosMatch(string text, string url, AzureDevOpsApi api, Images images)
        : base(url, api)
    {
        _images = images;

        Text = text;
    }

    protected override IMatch CreateMatch(GraphUserDto dto)
    {
        return new TeamsVideoMatch(dto, _images);
    }
}
