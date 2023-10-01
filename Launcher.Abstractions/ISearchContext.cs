namespace Launcher.Abstractions;

public interface ISearchContext
{
    IServiceProvider ServiceProvider { get; }

    IDictionary<string, object> Context { get; }

    Task DebounceDelay(CancellationToken cancellationToken);

    IEnumerable<IMatch> Filter(IEnumerable<IMatch> matches);

    IEnumerable<string> Prefilter(IEnumerable<string> matches);
}
