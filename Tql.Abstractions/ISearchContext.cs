namespace Tql.Abstractions;

public interface ISearchContext
{
    IServiceProvider ServiceProvider { get; }

    IDictionary<string, object> Context { get; }

    Task DebounceDelay(CancellationToken cancellationToken);

    IEnumerable<IMatch> Filter(IEnumerable<IMatch> matches);

    void SuppressPreliminaryResults();
}
