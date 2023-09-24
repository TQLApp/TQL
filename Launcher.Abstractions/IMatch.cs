namespace Launcher.Abstractions;

public interface IMatch
{
    string Text { get; }
    ICategory Category { get; }
    ICategory? ChildCategory { get; }
}
