namespace Tql.Abstractions;

public interface IPeopleDirectoryManager
{
    ImmutableArray<IPeopleDirectory> Directories { get; }

    void Add(IPeopleDirectory directory);
    void Remove(IPeopleDirectory directory);
}
