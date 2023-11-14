namespace Tql.Abstractions;

public interface ISearchableMatch : IMatch
{
    string SearchHint { get; }

    Task<IEnumerable<IMatch>> Search(
        ISearchContext context,
        string text,
        CancellationToken cancellationToken
    );
}
