namespace Tql.Abstractions;

public interface ISearchContext
{
    IServiceProvider ServiceProvider { get; }

    IDictionary<string, object> Context { get; }

    Task DebounceDelay(CancellationToken cancellationToken);

    IEnumerable<IMatch> Filter(IEnumerable<IMatch> matches, int? maxResults = null);

    Task<IEnumerable<IMatch>> FilterAsync(IEnumerable<IMatch> matches, int? maxResults = null);

    void SuppressPreliminaryResults();
}
