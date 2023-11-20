using System.IO;
using Microsoft.Extensions.Logging;
using Tql.Abstractions;
using Path = System.IO.Path;

namespace Tql.Utilities;

/// <summary>
/// Provides cache management for icons.
/// </summary>
/// <remarks>
/// This class provides a base implementation for implementing an icon cache.
/// The intended use of this class is to inherit from it, and use a DTO
/// type as the generic parameter. You can then implement the <see cref="LoadIcon(T)"/>
/// method to load an icon from a the server on demand. This class takes
/// care of caching the icons on disk and returning them as <see cref="ImageSource"/>
/// objects.
/// </remarks>
/// <typeparam name="T">Key type for icons.</typeparam>
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

    /// <summary>
    /// Occurs when an icon completes loading.
    /// </summary>
    public event EventHandler? IconLoaded;

    /// <summary>
    /// Initializes a new <see cref="IconCacheManager{T}"/>.
    /// </summary>
    /// <param name="store">Store service.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="configuration">Configuration.</param>
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

    /// <summary>
    /// Get an icon by the specified key.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method is synchronous. If the icon is present in the memory cache,
    /// or on disk, it's returned immediately. Otherwise an asynchronous background
    /// task is started to load the icon from the server.
    /// </para>
    ///
    /// <para>
    /// The intended use of this method is to call it in <see cref="IMatch"/>
    /// constructors and, if the image isn't available, fall back to a default
    /// icon. Then, next time the user finds the same match, the icon will be
    /// present in cache.
    /// </para>
    /// </remarks>
    /// <param name="key">Key to get the icon by.</param>
    /// <returns>Image source for the icon.</returns>
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

            return cache.ImageSource;
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

    /// <summary>
    /// Loads the icon from the server.
    /// </summary>
    /// <param name="key">Key of the icon to load.</param>
    /// <returns>Loaded icon.</returns>
    protected abstract Task<IconData> LoadIcon(T key);

    /// <summary>
    /// Raises the <see cref="IconLoaded"/> event.
    /// </summary>
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

/// <summary>
/// Configuration for the <see cref="IconCacheManager{T}"/> class.
/// </summary>
/// <param name="PluginId">ID of the plugin.</param>
/// <param name="Expiration">Expiration interval for icons.</param>
public record IconCacheManagerConfiguration(Guid PluginId, TimeSpan Expiration);

/// <summary>
/// Data of an icon.
/// </summary>
/// <remarks>
/// If the media type is <c>image/svg+xml</c>, the data will be
/// interpreted as an SVG icon. Otherwise the data is assumed to be an
/// image.
/// </remarks>
/// <param name="MediaType">Media type of the data.</param>
/// <param name="Data">Data of the icon.</param>
public record IconData(string MediaType, byte[] Data);
