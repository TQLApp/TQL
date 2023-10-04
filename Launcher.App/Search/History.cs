using Launcher.Abstractions;
using Launcher.App.Services.Database;

namespace Launcher.App.Search;

internal class History
{
    private readonly Dictionary<string, (HistoryEntity History, IMatch Match)> _byJson;

    public List<(HistoryEntity History, IMatch Match)> Items { get; }

    public History(IEnumerable<(HistoryEntity History, IMatch Match)> items)
    {
        Items = items.ToList();

        _byJson = Items.ToDictionary(p => p.History.Json!, p => p);
    }

    public (HistoryEntity History, IMatch Match)? GetByJson(MatchTypeId typeId, string json)
    {
        if (_byJson.TryGetValue(json, out var result))
            return result;

        return default;
    }
}
