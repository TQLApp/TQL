namespace Tql.App.Support;

internal class BucketSorter<T>
{
    private readonly List<T>?[] _buckets;

    public BucketSorter(int buckets)
    {
        _buckets = new List<T>[buckets];
    }

    public void Add(T item, int bucket)
    {
        var list = (_buckets[bucket] ??= new List<T>());

        list.Add(item);
    }

    public ImmutableArray<T> ToImmutableArray()
    {
        var size = 0;

        foreach (var bucket in _buckets)
        {
            if (bucket != null)
                size += bucket.Count;
        }

        if (size == 0)
            return ImmutableArray<T>.Empty;

        var builder = ImmutableArray.CreateBuilder<T>(size);

        foreach (var bucket in _buckets)
        {
            if (bucket != null)
                builder.AddRange(bucket);
        }

        return builder.ToImmutable();
    }
}
