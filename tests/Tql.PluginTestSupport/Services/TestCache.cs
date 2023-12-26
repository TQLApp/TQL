using System.IO;
using System.Text.Json;
using Tql.Abstractions;
using Tql.Utilities;

namespace Tql.PluginTestSupport.Services;

internal class TestCache<T> : ICache<T>
{
    private readonly ICacheManager<T> _cacheManager;
    private T _data = default!;
    private readonly object _syncRoot = new();

    public bool IsAvailable => true;

    public event EventHandler<CacheEventArgs<T>>? Updated;

    public TestCache(ICacheManager<T> cacheManager)
    {
        _cacheManager = cacheManager;

        if (!LoadFromDisk())
            Create();

        _cacheManager.CacheInvalidationRequired += (_, _) => Create();
    }

    private string GetCacheFileName()
    {
        Directory.CreateDirectory("Cache");

        return Path.Combine(
            "Cache",
            Encryption.Sha1Hash(typeof(T).FullName!) + "-v" + _cacheManager.Version
        );
    }

    private bool LoadFromDisk()
    {
        var cacheFileName = GetCacheFileName();
        if (!File.Exists(cacheFileName))
            return false;

        using var stream = File.OpenRead(cacheFileName);

        _data = JsonSerializer.Deserialize<T>(stream)!;

        return true;
    }

    public void Invalidate()
    {
        Create();
    }

    private void Create()
    {
        T data;

        lock (_syncRoot)
        {
            data = _cacheManager.Create().Result;

            _data = data;

            using var stream = File.Create(GetCacheFileName());

            JsonSerializer.Serialize(stream, _data);
        }

        OnUpdated(new CacheEventArgs<T>(data));
    }

    public Task<T> Get() => Task.FromResult(_data);

    public void RaiseUpdated()
    {
        OnUpdated(new CacheEventArgs<T>(_data));
    }

    protected virtual void OnUpdated(CacheEventArgs<T> e) => Updated?.Invoke(this, e);
}
