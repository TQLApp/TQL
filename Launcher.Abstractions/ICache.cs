namespace Launcher.Abstractions;

public interface ICache<T>
{
    bool IsAvailable { get; }

    Task<T> Get();
}
