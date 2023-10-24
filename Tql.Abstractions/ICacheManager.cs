namespace Tql.Abstractions;

public interface ICacheManager<T>
{
    int Version { get; }

    event EventHandler<CacheExpiredEventArgs>? CacheExpired;

    Task<T> Create();
}
