namespace Launcher.Abstractions;

public interface ISearchableMatch : IMatch
{
    Task<IEnumerable<IMatch>> Search(
        ISearchContext context,
        string text,
        CancellationToken cancellationToken
    );
}
