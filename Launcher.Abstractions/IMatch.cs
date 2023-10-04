namespace Launcher.Abstractions;

public interface IMatch
{
    string Text { get; }
    IImage Icon { get; }
    MatchTypeId TypeId { get; }
}
