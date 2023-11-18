using Microsoft.Extensions.Logging;
using Tql.Abstractions;
using Tql.App.Services.Database;
using Tql.App.Services.Telemetry;
using Tql.App.Support;

namespace Tql.App.Services;

internal class Cache<T> : ICache<T>
{
    private readonly ILogger<Cache<T>> _logger;
    private readonly ICacheManager<T> _cacheManager;
    private readonly IDb _db;
    private readonly CacheManagerManager _cacheManagerManager;
    private readonly TelemetryService _telemetryService;
    private TaskCompletionSource<T> _tcs = new();
    private bool _isFaulted;
    private DateTime _updated;
    private readonly object _syncRoot = new();
    private bool _creating;
    private TimeSpan _expiration;

    public bool IsAvailable
    {
        get
        {
            lock (_syncRoot)
            {
                return _tcs.Task.IsCompleted;
            }
        }
    }

    public event EventHandler<CacheEventArgs<T>>? Updated;

    public Cache(
        ILogger<Cache<T>> logger,
        ICacheManager<T> cacheManager,
        IDb db,
        CacheManagerManager cacheManagerManager,
        Settings settings,
        TelemetryService telemetryService
    )
    {
        _logger = logger;
        _cacheManager = cacheManager;
        _db = db;
        _cacheManagerManager = cacheManagerManager;
        _telemetryService = telemetryService;

        _expiration = GetExpiration();

        settings.AttachPropertyChanged(
            nameof(settings.CacheUpdateInterval),
            (_, _) => _expiration = GetExpiration()
        );

        cacheManagerManager.Register(this);

        LoadFromDb();

        if (!IsAvailable)
            Create(true);

        _cacheManager.CacheInvalidationRequired += (_, e) =>
        {
            if (e.Force)
            {
                lock (_syncRoot)
                {
                    _tcs = new TaskCompletionSource<T>();
                }
            }

            Create(e.Force);
        };

        TimeSpan GetExpiration()
        {
            return TimeSpan.FromMinutes(
                settings.CacheUpdateInterval ?? Settings.DefaultCacheUpdateInterval
            );
        }
    }

    private void LoadFromDb()
    {
        using var access = _db.Access();

        var cache = access.GetCache(typeof(T).FullName!);

        if (cache != null && cache.Version == _cacheManager.Version)
        {
            _logger.LogInformation("Reloading cache");

            try
            {
                _tcs.SetResult(JsonSerializer.Deserialize<T>(cache.Value!)!);

                _updated = cache.Updated!.Value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Reloading cache failed");
            }
        }
    }

    public void Invalidate()
    {
        Create(false);
    }

    private void Create(bool initialLoad)
    {
        lock (_syncRoot)
        {
            if (_creating)
                return;
            _creating = true;
        }

        _cacheManagerManager.StartLoading();

        Task.Run(async () =>
        {
            using var telemetry = _telemetryService.CreateDependency("Cache Update");

            telemetry.AddProperty("Type", typeof(T).FullName!);
            telemetry.AddProperty("Version", _cacheManager.Version.ToString());

            _logger.LogInformation("Recreating cache");

            var raiseCacheChanged = false;
            var now = DateTime.UtcNow;

            try
            {
                var data = await _cacheManager.Create();

                using (var access = _db.Access())
                {
                    access.SetCache(
                        new CacheEntity
                        {
                            Key = typeof(T).FullName,
                            Value = JsonSerializer.Serialize(data),
                            Version = _cacheManager.Version,
                            Updated = now
                        }
                    );
                }

                lock (_syncRoot)
                {
                    _isFaulted = false;

                    if (initialLoad)
                    {
                        _tcs.SetResult(data);
                    }
                    else
                    {
                        var tcs = new TaskCompletionSource<T>();

                        tcs.SetResult(data);

                        _tcs = tcs;
                    }

                    _updated = now;
                    raiseCacheChanged = true;
                }

                ThreadPool.QueueUserWorkItem(_ =>
                {
                    _logger.LogInformation("Raising cache updated");

                    try
                    {
                        OnUpdated(new CacheEventArgs<T>(data));
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Raising cache updated failed");
                    }
                });

                telemetry.IsSuccess = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to recreate cache");

                lock (_syncRoot)
                {
                    var wasFaulted = _isFaulted;
                    _isFaulted = true;

                    // On initial load, the TCS holding the cache will still be waiting
                    // for activation. If we don't activate it here (by faulting it), all
                    // searches depending on this cache will just get stuck.
                    //
                    // We only replace the TCS on cache refreshes if the cache already was
                    // faulted. This is to ensure that the user sees the last error message.
                    // He may be trying to fix the error but encountering a new one.

                    if (initialLoad)
                    {
                        _tcs.SetException(ex);

                        _updated = now;
                        raiseCacheChanged = true;
                    }
                    else if (wasFaulted)
                    {
                        var tcs = new TaskCompletionSource<T>();

                        tcs.SetException(ex);

                        _tcs = tcs;

                        // Don't raise cache changed. If the user is searching, and getting
                        // the cache raises the error, the search will get retried
                        // indefinitely.
                    }
                }
            }
            finally
            {
                lock (_syncRoot)
                {
                    _creating = false;
                }

                _cacheManagerManager.StopLoading();

                if (raiseCacheChanged)
                    _cacheManagerManager.RaiseCacheChanged();

                _logger.LogInformation("Recreating cache complete");
            }
        });
    }

    public Task<T> Get()
    {
        lock (_syncRoot)
        {
            // Recreate the cache if it's out of date or if the cache update failed.

            if (
                !_creating
                && IsAvailable
                && (_isFaulted || _updated < DateTime.UtcNow - _expiration)
            )
                Invalidate();

            return _tcs.Task;
        }
    }

    protected virtual void OnUpdated(CacheEventArgs<T> e) => Updated?.Invoke(this, e);
}
