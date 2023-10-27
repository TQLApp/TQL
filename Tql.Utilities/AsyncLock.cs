namespace Tql.Utilities;

public class AsyncLock : IDisposable
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private bool _disposed;

    public IDisposable Lock()
    {
        _semaphore.Wait();

        return new Release(this);
    }

    public async Task<IDisposable> LockAsync()
    {
        await _semaphore.WaitAsync();

        return new Release(this);
    }

    public async Task<IDisposable> LockAsync(CancellationToken cancellationToken)
    {
        await _semaphore.WaitAsync(cancellationToken);

        return new Release(this);
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _semaphore.Dispose();
            _disposed = true;
        }
    }

    private class Release : IDisposable
    {
        private readonly AsyncLock _owner;
        private bool _disposed;

        public Release(AsyncLock owner)
        {
            _owner = owner;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _owner.Dispose();
                _disposed = true;
            }
        }
    }
}
