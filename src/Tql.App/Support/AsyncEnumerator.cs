namespace Tql.App.Support;

internal abstract class AsyncEnumerator<T> : IAsyncEnumerator<T>
{
    private IEnumerator<T>? _values;

    public T Current
    {
        get
        {
            if (_values == null)
            {
                throw new InvalidOperationException(
                    "MoveNextAsync must be called before getting a value"
                );
            }

            return _values.Current;
        }
    }

    protected abstract Task<IEnumerable<T>> GetValues();

    public async ValueTask<bool> MoveNextAsync()
    {
        _values ??= (await GetValues()).GetEnumerator();

        return _values.MoveNext();
    }

    public virtual ValueTask DisposeAsync()
    {
        _values?.Dispose();

        return default;
    }
}
