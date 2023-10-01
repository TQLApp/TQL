using Launcher.Abstractions;

namespace Launcher.App;

internal class MatchesEventArgs
{
    public ImmutableArray<IMatch> Matches { get; }

    public MatchesEventArgs(ImmutableArray<IMatch> matches)
    {
        Matches = matches;
    }
}
