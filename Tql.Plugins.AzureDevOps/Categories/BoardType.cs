using Tql.Abstractions;
using Tql.Utilities;

namespace Tql.Plugins.AzureDevOps.Categories;

internal class BoardType : IMatchType
{
    public Guid Id => TypeIds.Board.Id;

    public IMatch Deserialize(string json)
    {
        return new BoardMatch(JsonSerializer.Deserialize<BoardMatchDto>(json)!);
    }
}
