using Launcher.Abstractions;
using Launcher.Utilities;

namespace Launcher.Plugins.Azure.Categories;

internal class PortalType : IMatchType
{
    private readonly Images _images;
    private readonly IImageFactory _imageFactory;

    public Guid Id => TypeIds.Portal.Id;

    public PortalType(Images images, IImageFactory imageFactory)
    {
        _images = images;
        _imageFactory = imageFactory;
    }

    public IMatch Deserialize(string json)
    {
        return new PortalMatch(
            JsonSerializer.Deserialize<PortalMatchDto>(json)!,
            _images,
            _imageFactory
        );
    }
}
