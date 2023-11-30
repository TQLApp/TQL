using Tql.Abstractions;
using Tql.App.Services.Database;

namespace Tql.App.Search;

internal class History
{
    private readonly Dictionary<
        (MatchTypeId TypeId, string Json),
        (HistoryEntity History, IMatch Match)
    > _byJson;

    public List<(HistoryEntity History, IMatch Match)> Items { get; }

    public History(IEnumerable<(HistoryEntity History, IMatch Match)> items)
    {
        Items = items.ToList();

        _byJson = Items.ToDictionary(p => (p.Match.TypeId, p.History.Json!), p => p);
    }

    public (HistoryEntity History, IMatch Match)? GetByJson(MatchTypeId typeId, string json)
    {
        if (_byJson.TryGetValue((typeId, json), out var result))
            return result;

        return default;
    }
}
