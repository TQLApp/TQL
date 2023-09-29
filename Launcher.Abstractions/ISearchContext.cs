namespace Launcher.Abstractions;

public interface ISearchContext
{
    IServiceProvider ServiceProvider { get; }

    IEnumerable<IMatch> Filter(IEnumerable<IMatch> matches, string text);
}
