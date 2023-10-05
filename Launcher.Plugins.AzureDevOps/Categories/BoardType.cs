using Launcher.Abstractions;
using Launcher.Utilities;

namespace Launcher.Plugins.AzureDevOps.Categories;

internal class BoardType : IMatchType
{
    private readonly Images _images;

    public Guid Id => TypeIds.Board.Id;

    public BoardType(Images images)
    {
        _images = images;
    }

    public IMatch Deserialize(string json)
    {
        return new BoardMatch(JsonSerializer.Deserialize<BoardMatchDto>(json)!, _images);
    }
}
