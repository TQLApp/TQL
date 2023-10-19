namespace Tql.Abstractions;

public interface ICacheManager<T>
{
    int Version { get; }

    Task<T> Create();
}
