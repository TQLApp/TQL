namespace Launcher.Abstractions;

public interface ICategory
{
    Guid Id { get; }
    IImage Icon { get; }
    string Title { get; }

    Task<ImmutableArray<IMatch>> Search(
        ISearchContext context,
        string text,
        CancellationToken cancellationToken
    );
}
