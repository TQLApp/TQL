using Tql.Abstractions;

namespace Tql.App;

internal class MatchEventArgs(IMatch match) : EventArgs
{
    public IMatch Match { get; } = match;
}
