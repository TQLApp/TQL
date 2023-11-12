using Tql.Abstractions;

namespace Tql.App;

internal class MatchEventArgs : EventArgs
{
    public IMatch Match { get; }

    public MatchEventArgs(IMatch match)
    {
        Match = match;
    }
}
