namespace Tql.Abstractions;

public interface ICache<T>
{
    bool IsAvailable { get; }

    event EventHandler<CacheEventArgs<T>> Updated;

    Task<T> Get();

    void Invalidate();
}
