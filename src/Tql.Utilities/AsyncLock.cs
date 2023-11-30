namespace Tql.Utilities;

/// <summary>
/// Asynchronous lock to replace the <c>lock</c> keyword for asynchronous code.
/// </summary>
public class AsyncLock : IDisposable
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private bool _disposed;

    /// <summary>
    /// Acquires a lock.
    /// </summary>
    /// <returns>Handle to release the lock.</returns>
    public IDisposable Lock()
    {
        _semaphore.Wait();

        return new Release(this);
    }

    /// <summary>
    /// Acquires a lock.
    /// </summary>
    /// <returns>Handle to release the lock.</returns>
    public async Task<IDisposable> LockAsync()
    {
        await _semaphore.WaitAsync();

        return new Release(this);
    }

    /// <summary>
    /// Acquires a lock.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Handle to release the lock.</returns>
    public async Task<IDisposable> LockAsync(CancellationToken cancellationToken)
    {
        await _semaphore.WaitAsync(cancellationToken);

        return new Release(this);
    }

    /// <summary>
    /// Disposes the lock.
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            _semaphore.Dispose();
            _disposed = true;
        }
    }

    private class Release(AsyncLock owner) : IDisposable
    {
        private bool _disposed;

        public void Dispose()
        {
            if (!_disposed)
            {
                owner._semaphore.Release();
                _disposed = true;
            }
        }
    }
}
