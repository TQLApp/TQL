using Launcher.Abstractions;
using Launcher.Plugins.Azure.Services;
using Launcher.Plugins.Azure.Support;
using Launcher.Utilities;

namespace Launcher.Plugins.Azure.Categories;

[RootMatchType]
internal class PortalsType : IMatchType
{
    private readonly Images _images;
    private readonly ConnectionManager _connectionManager;
    private readonly AzureApi _api;
    private readonly IImageFactory _imageFactory;

    public Guid Id => TypeIds.Portals.Id;

    public PortalsType(
        Images images,
        ConnectionManager connectionManager,
        AzureApi api,
        IImageFactory imageFactory
    )
    {
        _images = images;
        _connectionManager = connectionManager;
        _api = api;
        _imageFactory = imageFactory;
    }

    public IMatch? Deserialize(string json)
    {
        var dto = JsonSerializer.Deserialize<RootItemDto>(json)!;

        if (!_connectionManager.Connections.Any(p => p.Id == dto.Id))
            return null;

        return new PortalsMatch(
            MatchUtils.GetMatchLabel("Azure Portal", _connectionManager, dto.Id),
            _images,
            dto.Id,
            _api,
            _imageFactory
        );
    }
}
