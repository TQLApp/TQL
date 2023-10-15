namespace Tql.Abstractions;

public interface ICacheManager<T>
{
    TimeSpan Expiration { get; }
    int Version { get; }

    Task<T> Create();
}
