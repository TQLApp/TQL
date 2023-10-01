using Launcher.Abstractions;

namespace Launcher.Utilities;

public class MatchTypeManager
{
    private readonly Dictionary<Guid, IMatchType> _matchTypes;

    public MatchTypeManager(IEnumerable<IMatchType> matchTypes)
    {
        _matchTypes = matchTypes.ToDictionary(p => p.Id, p => p);
    }

    public IMatch? Deserialize(Guid typeId, string text, string json)
    {
        if (!_matchTypes.TryGetValue(typeId, out var type))
            return null;

        return type.Deserialize(text, json);
    }
}
