namespace Tql.Abstractions;

public interface IPeopleDirectory
{
    string Id { get; }
    string Name { get; }

    Task<ImmutableArray<IPerson>> Find(
        string search,
        CancellationToken cancellationToken = default
    );
}
