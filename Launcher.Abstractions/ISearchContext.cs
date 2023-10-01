namespace Launcher.Abstractions;

public interface ISearchContext
{
    IServiceProvider ServiceProvider { get; }

    IDictionary<string, object> Context { get; }

    Task DebounceDelay();

    IEnumerable<IMatch> Filter(IEnumerable<IMatch> matches, string text);

    IEnumerable<string> Prefilter(IEnumerable<string> matches, string text);
}
