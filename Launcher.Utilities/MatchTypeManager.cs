using Launcher.Abstractions;

namespace Launcher.Utilities;

public class MatchTypeManager
{
    private readonly Dictionary<Guid, IMatchType> _matchTypes;

    public ImmutableArray<IMatchType> MatchTypes { get; }

    public MatchTypeManager(IEnumerable<IMatchType> matchTypes)
    {
        MatchTypes = matchTypes.ToImmutableArray();

        _matchTypes = MatchTypes.ToDictionary(p => p.Id, p => p);
    }

    public IMatch? Deserialize(Guid typeId, string json)
    {
        if (!_matchTypes.TryGetValue(typeId, out var type))
            return null;

        return type.Deserialize(json);
    }
}
