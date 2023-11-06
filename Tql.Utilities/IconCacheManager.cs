using Microsoft.Extensions.Logging;
using System.IO;
using System.Text.Json;
using System.Windows.Media;
using Tql.Abstractions;

namespace Tql.Utilities;

public abstract class IconCacheManager<T>
    where T : notnull
{
    // ReSharper disable once StaticMemberInGenericType
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromDays(1);

    private readonly ILogger _logger;
    private readonly IconCacheManagerConfiguration _configuration;
    private readonly object _syncRoot = new();
    private readonly Dictionary<T, Cache> _cache = new();
    private readonly AsyncBackgroundProcessor _processor;
    private readonly string _cachePath;

    public event EventHandler? IconLoaded;

    protected IconCacheManager(
        IStore store,
        ILogger logger,
        IconCacheManagerConfiguration configuration
    )
    {
        _logger = logger;
        _configuration = configuration;

        _cachePath = Path.Combine(store.GetCacheFolder(configuration.PluginId), "Icon Cache");
        Directory.CreateDirectory(_cachePath);

        _processor = new AsyncBackgroundProcessor(logger);
    }

    public ImageSource? GetIcon(T key)
    {
        _logger.LogDebug("Requested icon '{Url}'", key);

        lock (_syncRoot)
        {
            if (!_cache.TryGetValue(key, out var cache) || cache.Expiration < DateTime.Now)
            {
                var (imageSource, requireReload) = LoadFromDisk(key);

                if (requireReload)
                    QueueLoadIcon(key);

                // Refresh the cached icons regularly.

                var cacheExpiration =
                    _configuration.Expiration < CacheExpiration
                        ? CacheExpiration
                        : _configuration.Expiration;

                cache = new Cache(DateTime.Now + cacheExpiration) { ImageSource = imageSource };

                _cache[key] = cache;
            }

            return cache?.ImageSource;
        }
    }

    private void QueueLoadIcon(T key)
    {
        _logger.LogInformation("Fetching (new) icon '{Url}'", key);

        _processor.Enqueue(Load);

        async Task Load()
        {
            var data = await LoadIcon(key);

            // Called before writing the file to disk to ensure the
            // image is valid.
            var image = CreateImage(data);

            using (var stream = File.Create(GetCacheIconFileName(key)))
            {
                await JsonSerializer.SerializeAsync(stream, data);
            }

            lock (_syncRoot)
            {
                _cache[key].ImageSource = image;
            }

            _logger.LogInformation("Loading complete of icon '{Url}'", key);

            OnIconLoaded();
        }
    }

    private (ImageSource? ImageSource, bool RequireReload) LoadFromDisk(T key)
    {
        var fileName = GetCacheIconFileName(key);

        if (!File.Exists(fileName))
            return (null, true);

        var writeTime = File.GetLastWriteTime(fileName);

        bool isExpired = writeTime < DateTime.Now - _configuration.Expiration;

        try
        {
            IconData dto;

            using (var stream = File.OpenRead(fileName))
            {
                dto = JsonSerializer.Deserialize<IconData>(stream)!;
            }

            var imageSource = CreateImage(dto);

            return (imageSource, isExpired);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load icon from cache");

            return (null, true);
        }
    }

    private ImageSource CreateImage(IconData dto)
    {
        using var stream = new MemoryStream(dto.Data);

        if (dto.MediaType == "image/svg+xml")
            return ImageFactory.CreateSvgImage(stream);

        return ImageFactory.CreateBitmapImage(stream);
    }

    private string GetCacheIconFileName(T key)
    {
        return Path.Combine(_cachePath, Encryption.Sha1Hash(key.ToString()));
    }

    protected abstract Task<IconData> LoadIcon(T key);

    protected virtual void OnIconLoaded() => IconLoaded?.Invoke(this, EventArgs.Empty);

    private class Cache
    {
        public DateTime Expiration { get; }
        public ImageSource? ImageSource { get; set; }

        public Cache(DateTime expiration)
        {
            Expiration = expiration;
        }
    }
}

public record IconCacheManagerConfiguration(Guid PluginId, TimeSpan Expiration);

public record IconData(string MediaType, byte[] Data);
