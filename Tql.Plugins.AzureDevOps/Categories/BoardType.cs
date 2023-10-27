using Tql.Abstractions;
using Tql.Plugins.AzureDevOps.Services;
using Tql.Utilities;

namespace Tql.Plugins.AzureDevOps.Categories;

internal class BoardType : IMatchType
{
    private readonly ConfigurationManager _configurationManager;

    public Guid Id => TypeIds.Board.Id;

    public BoardType(ConfigurationManager configurationManager)
    {
        _configurationManager = configurationManager;
    }

    public IMatch? Deserialize(string json)
    {
        var dto = JsonSerializer.Deserialize<BoardMatchDto>(json)!;
        if (!_configurationManager.Configuration.HasConnection(dto.Url))
            return null;

        return new BoardMatch(dto);
    }
}
